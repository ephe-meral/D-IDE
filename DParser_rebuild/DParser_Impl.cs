﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace D_Parser
{
    /// <summary>
    /// Parser for D Code
    /// </summary>
    public partial class DParser
    {
        #region Modules
        // http://www.digitalmars.com/d/2.0/module.html

        /// <summary>
        /// Module entry point
        /// </summary>
        void Root()
        {
            Step();

            // Only one module declaration possible possible!
            if (LA(Module))
                ModuleDeclaration();

            // Now only declarations or other statements are allowed!
            while (!IsEOF)
            {
                DeclDef(ref doc);
            }
        }

        void DeclDef(ref DNode par)
        {
            //AttributeSpecifier
            if (IsAttributeSpecifier())
                AttributeSpecifier();

            //ImportDeclaration
            else if (LA(Import))
                ImportDeclaration();

            //EnumDeclaration
            else if (LA(Enum))
                EnumDeclaration();

            //ClassDeclaration
            else if (LA(Class))
                ClassDeclaration();

            //InterfaceDeclaration
            else if (LA(Interface))
                InterfaceDeclaration();

            //AggregateDeclaration
            else if (LA(Struct) || LA(Union))
                AggregateDeclaration();

            //Declaration
            else if (IsDeclaration())
                Declaration(ref par);

            //Constructor
            else if (LA(This))
                Constructor(ref par);

            //Destructor
            else if (LA(Tilde) && LA(This))
                Destructor(ref par);

            //Invariant
            //UnitTest
            //StaticConstructor
            //StaticDestructor
            //SharedStaticConstructor
            //SharedStaticDestructor
            //ConditionalDeclaration
            //StaticAssert
            //TemplateDeclaration
            //TemplateMixin

            //MixinDeclaration
            else if (LA(Mixin))
                MixinDeclaration();

            //;
            else if (LA(Semicolon))
                Step();

            // else:
            else
            {
                SynErr(t.Kind, "Declaration expected");
                Step();
            }
        }

        void ModuleDeclaration()
        {
            Expect(Module);
            Document.module = ModuleFullyQualifiedName();
            Expect(Semicolon);
        }

        string ModuleFullyQualifiedName()
        {
            Expect(Identifier);
            string ret = t.Value;

            while (la.Kind == Dot)
            {
                Step();
                Expect(Identifier);

                ret += "." + t.Value;
            }
            return ret;
        }

        void ImportDeclaration()
        {
            Expect(Import);

            _Import();

            // ImportBindings
            if (LA(Colon))
            {
                ImportBind();
                while (LA(Comma))
                {
                    Step();
                    ImportBind();
                }
            }
            else
                while (LA(Comma))
                {
                    Step();
                    _Import();
                }

            Expect(Semicolon);
        }

        void _Import()
        {
            string imp = "";

            // ModuleAliasIdentifier
            if (PK(Assign))
            {
                Expect(Identifier);
                string ModuleAliasIdentifier = t.Value;
                Step();
            }

            imp = ModuleFullyQualifiedName();
            import.Add(imp);
        }

        void ImportBind()
        {
            Expect(Identifier);
            string imbBind = t.Value;
            string imbBindDef = null;

            if (LA(Assign))
            {
                Step();
                imbBindDef = t.Value;
            }
        }


        void MixinDeclaration()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Declarations
        // http://www.digitalmars.com/d/2.0/declaration.html

        bool IsDeclaration()
        {
            return LA(Alias) || IsStorageClass || IsBasicType();
        }

        void Declaration(ref DNode par)
        {
            if (LA(Alias))
            {
                Step();
            }
            Decl(ref par);
        }

        void Decl(ref DNode par)
        {
            // Enum possible storage class attributes
            List<int> storAttr = new List<int>();
            while (IsStorageClass)
            {
                Step();
                storAttr.Add(t.Kind);
            }

            // Autodeclaration
            if (storAttr.Count > 0 && PK(Identifier) && Peek().Kind == DTokens.Assign)
            {
                Step();
                DNode n = new DVariable();
                n.Type = new DTokenDeclaration(t.Kind);
                n.TypeToken = t.Kind;
                Expect(Identifier);
                n.name = t.Value;
                Expect(Assign);
                (n as DVariable).Initializer = AssignExpression();
                Expect(Semicolon);
                par.Add(n);
                return;
            }

            // Declarators
            TypeDeclaration ttd = BasicType();
            DNode firstNode = Declarator(false);

            if (firstNode.Type == null)
                firstNode.Type = ttd;
            else
                firstNode.Type.MostBasic.Base = ttd;


            // BasicType Declarators ;
            bool ExpectFunctionBody = !(LA(Assign) || LA(Comma) || LA(Semicolon));
            if (!ExpectFunctionBody)
            {
                // DeclaratorInitializer
                if (LA(Assign))
                {
                    Step();
                    (firstNode as DVariable).Initializer = Initializer();
                }

                par.Add(firstNode);

                // DeclaratorIdentifierList
                while (LA(Comma))
                {
                    Step();
                    Expect(Identifier);

                    DVariable otherNode = new DVariable();
                    otherNode.Assign(firstNode);
                    otherNode.name = t.Value;

                    if (LA(Assign))
                        otherNode.Initializer = Initializer();

                    par.Add(otherNode);
                }

                Expect(Semicolon);
            }

            // BasicType Declarator FunctionBody
            else
            {
                FunctionBody(ref firstNode);
                par.Add(firstNode);
            }
        }

        bool IsBasicType()
        {
            return BasicTypes[la.Kind] || LA(Typeof) || MemberFunctionAttribute[la.Kind] || (LA(Dot) && PK(Identifier)) || LA(Identifier);
        }

        TypeDeclaration BasicType()
        {
            TypeDeclaration td = null;
            if (BasicTypes[la.Kind])
            {
                Step();
                return new DTokenDeclaration(t.Kind);
            }

            if (MemberFunctionAttribute[la.Kind])
            {
                Step();
                MemberFunctionAttributeDecl md = new MemberFunctionAttributeDecl(t.Kind);
                Expect(OpenParenthesis);
                md.InnerType = Type();
                Expect(CloseParenthesis);
                return md;
            }

            if (LA(Typeof))
            {
                td = TypeOf();
                if (!LA(Dot)) return td;
            }

            if (LA(Dot))
                Step();

            if (td == null)
                td = IdentifierList();
            else
                td.Base = IdentifierList();

            return td;
        }

        bool IsBasicType2()
        {
            return LA(Times) || LA(OpenSquareBracket) || LA(Delegate) || LA(Function);
        }

        TypeDeclaration BasicType2()
        {
            // *
            if (LA(Times))
            {
                Step();
                return new PointerDecl();
            }

            // [ ... ]
            else if (LA(OpenSquareBracket))
            {
                Step();
                // [ ]
                if (LA(CloseSquareBracket)) { Step(); return new ClampDecl(); }

                TypeDeclaration ret = null;

                // [ Type ]
                if (IsBasicType())
                    ret = Type();
                else
                {
                    ret = new DExpressionDecl(AssignExpression());

                    // [ AssignExpression .. AssignExpression ]
                    if (LA(Dot))
                    {
                        Step();
                        Expect(Dot);

                        //TODO: do something with the 2nd expression here
                        AssignExpression();
                    }
                }

                Expect(CloseSquareBracket);
                return ret;
            }

            // delegate | function
            else if (LA(Delegate) || LA(Function))
            {
                Step();
                TypeDeclaration td = null;
                DelegateDeclaration dd = new DelegateDeclaration();
                dd.IsFunction = t.Kind == Function;

                dd.Parameters = Parameters();
                td = dd;
                //TODO: add attributes to declaration
                while (FunctionAttribute[la.Kind])
                {
                    Step();
                    td = new DTokenDeclaration(t.Kind, td);
                }
                return td;
            }
            else
                SynErr(Identifier);
            return null;
        }

        /// <summary>
        /// Parses a type declarator
        /// </summary>
        /// <returns>A dummy node that contains the return type, the variable name and possible parameters of a function declaration</returns>
        DNode Declarator(bool IsParam)
        {
            DNode ret = new DVariable();
            TypeDeclaration ttd = null;

            while (IsBasicType2())
            {
                if (ret.Type == null) ret.Type = BasicType2();
                else { ttd = BasicType2(); ttd.Base = ret.Type; ret.Type = ttd; }
            }
            /*
             * Add some syntax possibilities here
             * like in 
             * int (x);
             * or in
             * int(*foo);
             */
            if (LA(OpenParenthesis))
            {
                Step();
                ClampDecl cd = new ClampDecl(ret.Type, ClampDecl.ClampType.Round);
                ret.Type = cd;

                /* 
                 * Parse all basictype2's that are following the initial '('
                 */
                while (IsBasicType2())
                {
                    ttd = BasicType2();

                    if (cd.KeyType == null) cd.KeyType = ttd;
                    else
                    {
                        ttd.Base = cd.KeyType;
                        cd.KeyType = ttd;
                    }
                }

                /*
                 * Here can be an identifier with some optional DeclaratorSuffixes
                 */
                if (!LA(CloseParenthesis))
                {
                    if (IsParam && !LA(Identifier))
                    {
                        /* If this Declarator is a parameter of a function, don't expect anything here
                         * exept a '*' that means that here's an anonymous function pointer
                         */
                        if (!T(Times))
                            SynErr(Times);
                    }
                    else
                    {
                        Expect(Identifier);
                        ret.name = t.Value;

                        /*
                         * Just here suffixes can follow!
                         */
                        if (!LA(CloseParenthesis))
                        {
                            List<DNode> _unused = null;
                            ttd = DeclaratorSuffixes(out _unused, out _unused);

                            if (cd.KeyType == null) cd.KeyType = ttd;
                            else
                            {
                                ttd.Base = cd.KeyType;
                                cd.KeyType = ttd;
                            }
                        }
                    }
                }
                ret.Type = cd;
                Expect(CloseParenthesis);
            }
            else
            {
                if (IsParam && !LA(Identifier))
                    return ret;

                Expect(Identifier);
                ret.name = t.Value;
            }

            if (IsDeclaratorSuffix)
            {
                // DeclaratorSuffixes
                List<DNode> _Parameters;
                ttd = DeclaratorSuffixes(out ret.TemplateParameters, out _Parameters);
                if (ttd != null)
                {
                    ttd.Base = ret.Type;
                    ret.Type = ttd;
                }

                if (_Parameters != null)
                {
                    DMethod dm = new DMethod();
                    dm.Assign(ret);
                    dm.Parameters = _Parameters;
                    ret = dm;
                }
            }

            return ret;
        }

        bool IsDeclaratorSuffix
        {
            get { return LA(OpenSquareBracket) || LA(OpenParenthesis); }
        }

        /// <summary>
        /// Note:
        /// http://www.digitalmars.com/d/2.0/declaration.html#DeclaratorSuffix
        /// The definition of a sequence of declarator suffixes is buggy here! Theoretically template parameters can be declared without a surrounding ( and )!
        /// Also, more than one parameter sequences are possible!
        /// 
        /// TemplateParameterList[opt] Parameters MemberFunctionAttributes[opt]
        /// </summary>
        TypeDeclaration DeclaratorSuffixes(out List<DNode> TemplateParameters, out List<DNode> _Parameters)
        {
            TypeDeclaration td = null;
            TemplateParameters = new List<DNode>();
            _Parameters = null;

            while (LA(OpenSquareBracket))
            {
                Step();
                ClampDecl ad = new ClampDecl(td);
                if (!LA(CloseSquareBracket))
                {
                    if (IsAssignExpression())
                        ad.KeyType = new DExpressionDecl(AssignExpression());
                    else
                        ad.KeyType = Type();
                }
                Expect(CloseSquareBracket);
                ad.ValueType = td;
                td = ad;
            }

            if (LA(OpenParenthesis))
            {
                if (IsTemplateParameterList())
                {
                    Step();
                    TemplateParameters = TemplateParameterList();
                    Expect(CloseParenthesis);
                }
                _Parameters = Parameters();

                //TODO: MemberFunctionAttributes -- add them to the declaration
                while (MemberFunctionAttribute[la.Kind])
                {
                    Step();
                }
            }
            return td;
        }

        TypeDeclaration IdentifierList()
        {
            TypeDeclaration td = null;

            if (!LA(Identifier))
                SynErr(Identifier);

            // Template instancing
            if (PK(Not))
                td = TemplateInstance();

            // Identifier
            else
            {
                Step();
                td = new NormalDeclaration(t.Value);
            }

            // IdentifierList
            while (LA(Dot))
            {
                Step();
                DotCombinedDeclaration dcd = new DotCombinedDeclaration(td);
                // Template instancing
                if (PK(Not))
                    dcd.AccessedMember = TemplateInstance();
                // Identifier
                else
                    dcd.AccessedMember = new NormalDeclaration(t.Value);
                td = dcd;
            }

            return td;
        }

        bool IsStorageClass
        {
            get
            {
                return LA(Abstract) ||
            LA(Auto) ||
            LA(Const) ||
            LA(Deprecated) ||
            LA(Extern) ||
            LA(Final) ||
            LA(Immutable) ||
            LA(InOut) ||
            LA(Shared) ||
        LA(Nothrow) ||
            LA(Override) ||
        LA(Pure) ||
            LA(Scope) ||
            LA(Static) ||
            LA(Synchronized);
            }
        }

        TypeDeclaration Type()
        {
            TypeDeclaration td = BasicType();

            if (IsDeclarator2())
            {
                TypeDeclaration ttd = Declarator2();
                ttd.Base = td;
                td = ttd;
            }

            return td;
        }

        bool IsDeclarator2()
        {
            return IsBasicType2() || LA(OpenParenthesis);
        }

        /// <summary>
        /// http://www.digitalmars.com/d/2.0/declaration.html#Declarator2
        /// The next bug: Following the definition strictly, this function would end up in an endless loop of requesting another Declarator2
        /// 
        /// So here I think that a Declarator2 only consists of a couple of BasicType2's and some DeclaratorSuffixes
        /// </summary>
        /// <returns></returns>
        TypeDeclaration Declarator2()
        {
            TypeDeclaration td = null;
            if (LA(OpenParenthesis))
            {
                Step();
                td = Declarator2();
                Expect(CloseParenthesis);

                // DeclaratorSuffixes
                if (LA(OpenSquareBracket))
                {
                    List<DNode> _unused = null, _unused2 = null;
                    DeclaratorSuffixes(out _unused, out _unused2);
                }
                return td;
            }

            while (IsBasicType2())
            {
                TypeDeclaration ttd = BasicType2();
                ttd.Base = td;
                td = ttd;
            }

            return null;
        }

        /// <summary>
        /// Parse parameters
        /// </summary>
        List<DNode> Parameters()
        {
            List<DNode> ret = new List<DNode>();
            Expect(OpenParenthesis);

            // Empty parameter list
            if (LA(CloseParenthesis))
            {
                Step();
                return ret;
            }

            if (!IsTripleDot())
                ret.Add(Parameter());

            while (LA(Comma))
            {
                Step();
                if (IsTripleDot())
                    break;
                ret.Add(Parameter());
            }

            /*
             * There can be only one '...' in every parameter list
             */
            if (IsTripleDot())
            {
                // If it had not a comma, add a VarArgDecl to the last parameter
                bool HadComma = T(Comma);

                Step();
                Step();
                Step();

                if (!HadComma && ret.Count > 0)
                {
                    ret[ret.Count - 1].Type = new VarArgDecl(ret[ret.Count - 1].Type);
                }
                else
                {
                    DVariable dv = new DVariable();
                    dv.Type = new VarArgDecl();
                    ret.Add(dv);
                }
            }

            Expect(CloseParenthesis);
            return ret;
        }

        bool IsTripleDot()
        {
            return LA(Dot) && PK(Dot) && Peek().Kind == Dot;
        }

        private DNode Parameter()
        {
            //TODO: Handle this
            if (IsInOut())
            {
                Step();
            }

            TypeDeclaration td = null;
            /*
             * A basictype is possible(!), not required
             */
            if (IsBasicType() && (!LA(Identifier) || (!PK(Identifier) && lexer.CurrentPeekToken.Kind != OpenParenthesis)))
            {
                td = BasicType();
            }

            DNode ret = Declarator(true);
            if (ret.Type == null)
                ret.Type = td;
            else
                ret.Type.Base = td;

            // DefaultInitializerExpression
            if (LA(Assign))
            {
                Step();
                DExpression defInit = null;
                if (LA(Identifier) && (la.Value == "__FILE__" || la.Value == "__LINE__"))
                    defInit = new IdentExpression(la.Value);
                else
                    defInit = AssignExpression();

                if (ret is DVariable)
                    (ret as DVariable).Initializer = defInit;
            }

            return ret;
        }

        bool IsInOut()
        {
            return LA(In) || LA(Out) || LA(Ref) || LA(Lazy);
        }


        private DExpression Initializer()
        {
            // VoidInitializer
            if (LA(Void))
            {
                Step();
                return new TokenExpression(Void);
            }

            return NonVoidInitializer();
        }

        DExpression NonVoidInitializer()
        {
            // ArrayInitializer | StructInitializer
            if (LA(OpenSquareBracket) || LA(OpenCurlyBrace))
            {
                Step();
                bool IsStructInit = T(OpenCurlyBrace);
                if (IsStructInit ? LA(CloseCurlyBrace) : LA(CloseSquareBracket))
                {
                    Step();
                    return new ClampExpression(IsStructInit ? ClampExpression.ClampType.Curly : ClampExpression.ClampType.Square);
                }

                // ArrayMemberInitializations
                ArrayExpression ae = new ArrayExpression(IsStructInit ? ClampExpression.ClampType.Curly : ClampExpression.ClampType.Square);
                DExpression element = null;

                bool IsInit = true;
                while (IsInit || LA(Comma))
                {
                    if (!IsInit) Step();
                    IsInit = false;


                    if (IsStructInit)
                    {
                        // Identifier : NonVoidInitializer
                        if (LA(Identifier) && PK(Colon))
                        {
                            Step();
                            AssignTokenExpression inh = new AssignTokenExpression(Colon);
                            inh.PrevExpression = new IdentExpression(t.Value);
                            Step();
                            inh.FollowingExpression = NonVoidInitializer();
                            element = inh;
                        }
                        else
                            element = NonVoidInitializer();
                    }
                    else
                    {
                        // ArrayMemberInitialization
                        element = NonVoidInitializer();
                        bool HasBeenAssExpr = !(T(CloseSquareBracket) || T(CloseCurlyBrace));

                        // AssignExpression : NonVoidInitializer
                        if (HasBeenAssExpr && LA(Colon))
                        {
                            Step();
                            AssignTokenExpression inhExpr = new AssignTokenExpression(Colon);
                            inhExpr.PrevExpression = element;
                            inhExpr.FollowingExpression = NonVoidInitializer();
                            element = inhExpr;
                        }
                    }

                    ae.Expressions.Add(element);
                }

                Expect(CloseSquareBracket);
                return ae;
            }
            else
                return AssignExpression();
        }

        TypeDeclaration TypeOf()
        {
            Expect(Typeof);
            Expect(OpenParenthesis);
            MemberFunctionAttributeDecl md = new MemberFunctionAttributeDecl(Typeof);
            if (LA(Return))
                md.InnerType = new DTokenDeclaration(Return);
            else
                md.InnerType = new DExpressionDecl(Expression());
            Expect(CloseParenthesis);
            return md;
        }

        #endregion

        #region Attributes

        void _Pragma()
        {
            Expect(Pragma);
            Expect(OpenParenthesis);
            Expect(Identifier);

            if (LA(Comma))
            {
                Step();
                ArgumentList();
            }
            Expect(CloseParenthesis);
        }

        bool IsAttributeSpecifier()
        {
            return (LA(Extern) || LA(Align) || LA(Pragma) || LA(Deprecated) || IsProtectionAttribute()
                || LA(Static) || LA(Final) || LA(Override) || LA(Abstract) || LA(Const) || LA(Auto) || LA(Scope) || LA(__gshared) || LA(Shared) || LA(Immutable) || LA(InOut)
                || LA(DisabledAttribute));
        }

        bool IsProtectionAttribute()
        {
            return LA(Public) || LA(Private) || LA(Protected) || LA(Extern) || LA(Package);
        }

        private void AttributeSpecifier()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Expressions
        DExpression Expression()
        {
            // AssignExpression
            DExpression ass = AssignExpression();
            if (!LA(Comma))
                return ass;

            /*
             * The following is a leftover of C syntax and proably cause some errors when parsing arguments etc.
             */
            // AssignExpression , Expression
            ArrayExpression ae = new ArrayExpression(ClampExpression.ClampType.Round);
            ae.Expressions.Add(ass);
            while (LA(Comma))
            {
                Step();
                ae.Expressions.Add(AssignExpression());
            }
            return ae;
        }

        bool IsAssignExpression()
        {
            return false;
        }

        DExpression AssignExpression()
        {
            DExpression left = ConditionalExpression();
            if (!AssignOps[la.Kind])
                return left;

            AssignTokenExpression ate = new AssignTokenExpression(t.Kind);
            ate.PrevExpression = left;
            ate.FollowingExpression = AssignExpression();
            return ate;
        }

        DExpression ConditionalExpression()
        {
            DExpression trigger = OrOrExpression();
            if (!LA(Question))
                return trigger;

            Expect(Question);
            SwitchExpression se = new SwitchExpression(trigger);
            se.TrueCase = AssignExpression();
            Expect(Colon);
            se.FalseCase = ConditionalExpression();
            return se;
        }

        DExpression OrOrExpression()
        {
            DExpression left = CmpExpression();
            if (!(LA(LogicalOr) || LA(LogicalAnd) || LA(BitwiseOr) || LA(BitwiseAnd) || LA(Xor)))
                return left;

            Step();
            AssignTokenExpression ae = new AssignTokenExpression(t.Kind);
            ae.PrevExpression = left;
            ae.FollowingExpression = OrOrExpression();
            return ae;
        }

        DExpression CmpExpression()
        {
            DExpression left = AddExpression();

            bool CanProceed =
                // RelExpression
                RelationalOperators[la.Kind] ||
                // EqualExpression
                LA(Equal) || LA(NotEqual) ||
                // IdentityExpression | InExpression
                LA(Is) || LA(In) || (LA(Not) && (PK(Is) || lexer.CurrentPeekToken.Kind == In)) ||
                // ShiftExpression
                LA(ShiftLeft) || LA(ShiftRight) || LA(ShiftRightUnsigned);

            if (!CanProceed)
                return left;

            // If we have a !in or !is
            if (LA(Not)) Step();
            Step();
            AssignTokenExpression ae = new AssignTokenExpression(t.Kind);
            ae.PrevExpression = left;
            // When a shift expression occurs, an AddExpression is required to follow
            if (T(ShiftLeft) || T(ShiftRight) || T(ShiftRightUnsigned))
                ae.FollowingExpression = AddExpression();
            else
                ae.FollowingExpression = OrOrExpression();
            return ae;
        }

        private DExpression AddExpression()
        {
            DExpression left = MulExpression();

            if (!(LA(Plus) || LA(Minus) || LA(Tilde)))
                return left;

            Step();
            AssignTokenExpression ae = new AssignTokenExpression(t.Kind);
            ae.PrevExpression = left;
            ae.FollowingExpression = MulExpression();
            return ae;
        }

        DExpression MulExpression()
        {
            DExpression left = PowExpression();

            if (!(LA(Times) || LA(Div) || LA(Mod)))
                return left;

            Step();
            AssignTokenExpression ae = new AssignTokenExpression(t.Kind);
            ae.PrevExpression = left;
            ae.FollowingExpression = MulExpression();
            return ae;
        }

        DExpression PowExpression()
        {
            DExpression left = UnaryExpression();

            if (!(LA(Pow)))
                return left;

            Step();
            AssignTokenExpression ae = new AssignTokenExpression(t.Kind);
            ae.PrevExpression = left;
            ae.FollowingExpression = PowExpression();
            return ae;
        }

        DExpression UnaryExpression()
        {
            // CastExpression
            if (LA(Cast))
            {
                Step();
                Expect(OpenParenthesis);
                TypeDeclaration castType = Type();
                Expect(CloseParenthesis);

                DExpression ex = UnaryExpression();
                ClampExpression ce = new ClampExpression(new TokenExpression(Cast), ClampExpression.ClampType.Round);
                ex.Base = ce;
                return ex;
            }

            if (LA(BitwiseAnd) || LA(Increment) || LA(Decrement) || LA(Times) || LA(Minus) || LA(Plus) ||
                LA(Not) || LA(Tilde))
            {
                Step();
                AssignTokenExpression ae = new AssignTokenExpression(t.kind);
                ae.FollowingExpression = UnaryExpression();
                return ae;
            }

            if (LA(OpenParenthesis))
            {
                Step();
                ClampExpression ce = new ClampExpression(ClampExpression.ClampType.Round);
                ce.InnerExpression = new TypeDeclarationExpression(Type());
                Expect(CloseParenthesis);
                Expect(Dot);
                Expect(Identifier);
                AssignTokenExpression ae = new AssignTokenExpression(Dot);
                ae.PrevExpression = ce;
                ae.FollowingExpression = new IdentExpression(t.Value);
                return ae;
            }

            // NewExpression
            if (LA(New))
                return NewExpression();

            // DeleteExpression
            if (LA(Delete))
            {
                Step();
                DExpression ex = UnaryExpression();
                ex.Base = new TokenExpression(Delete);
                return ex;
            }

            return PostfixExpression();
        }

        DExpression NewExpression()
        {
            Expect(New);
            DExpression ex = new TokenExpression(New);

            // NewArguments
            if (LA(OpenParenthesis))
            {
                Step();
                if (LA(CloseParenthesis))
                    Step();
                else
                {
                    ArrayExpression ae = new ArrayExpression(ClampExpression.ClampType.Round);
                    ae.Base = ex;
                    ae.Expressions = ArgumentList();
                    ex = ae;
                }
            }

            /*
             * If here occurs a class keyword, interpretate it as an anonymous class definition
             * NewArguments ClassArguments BaseClasslist[opt] { DeclDefs } 
             */
            if (LA(Class))
            {
                Step();
                DExpression ex2 = new TokenExpression(Class);
                ex2.Base = ex;
                ex = ex2;

                // ClassArguments
                if (LA(OpenParenthesis))
                {
                    if (LA(CloseParenthesis))
                        Step();
                    else
                    {
                        ArrayExpression ae = new ArrayExpression(ClampExpression.ClampType.Round);
                        ae.Base = ex;
                        ae.Expressions = ArgumentList();
                        ex = ae;
                    }
                }

                // BaseClasslist[opt]
                if (LA(Colon))
                {
                    //TODO : Add base classes to expression somehow ;-)
                    BaseClassList();
                }

                Expect(OpenCurlyBrace);

                //TODO : Add anonymous class to parent expression somehow
                DNode anonClass = new DClassLike();
                anonClass.TypeToken = Class;
                while (!IsEOF && !LA(CloseCurlyBrace))
                {
                    DeclDef(ref anonClass);
                }

                Expect(CloseCurlyBrace);
                return ex;
            }

            // NewArguments Type
            else
            {
                DExpression ex2 = new TypeDeclarationExpression(Type());
                ex2.Base = ex;
                ex = ex2;

                if (LA(OpenSquareBracket))
                {
                    ClampExpression ce = new ClampExpression();
                    ce.Base = ex;
                    ce.InnerExpression = AssignExpression();
                    Expect(CloseSquareBracket);
                }
                else if (LA(OpenParenthesis))
                {
                    ArrayExpression ae = new ArrayExpression(ClampExpression.ClampType.Round);
                    ae.Base = ex;
                    ae.Expressions = ArgumentList();
                    ex = ae;
                }
            }
            return ex;
        }

        List<DExpression> ArgumentList()
        {
            List<DExpression> ret = new List<DExpression>();

            ret.Add(AssignExpression());

            while (LA(Comma))
            {
                Step();
                ret.Add(AssignExpression());
            }

            return ret;
        }

        DExpression PostfixExpression()
        {
            // PostfixExpression
            DExpression retEx = PrimaryExpression();

            /*
             * A postfixexpression must start with a primaryexpression and can 
             * consist of more than one additional epxression --
             * things like foo()[1] become possible then
             */
            while (LA(Dot) || LA(Increment) || LA(Decrement) || LA(OpenParenthesis) || LA(OpenSquareBracket))
            {
                // Function call
                if (LA(OpenParenthesis))
                {
                    Step();
                    ArrayExpression ae = new ArrayExpression(ClampExpression.ClampType.Round);
                    ae.Base = retEx;
                    if (!LA(CloseParenthesis))
                        ae.Expressions = ArgumentList();
                    else Step();

                    retEx = ae;
                }

                // IndexExpression | SliceExpression
                else if (LA(OpenSquareBracket))
                {
                    Step();

                    if (!LA(CloseSquareBracket))
                    {
                        DExpression firstEx = AssignExpression();
                        // [ AssignExpression .. AssignExpression ]
                        if (LA(Dot) && PK(Dot))
                        {
                            Step();
                            Step();
                            TokenExpression tex = new TokenExpression(Dot);
                            tex.Base = firstEx;
                            TokenExpression tex2 = new TokenExpression(Dot);
                            tex2.Base = tex;

                            DExpression second = AssignExpression();
                            second.Base = tex2;

                            retEx = second;
                        }
                        // [ ArgumentList ]
                        else if (LA(Comma))
                        {
                            ArrayExpression ae = new ArrayExpression();
                            ae.Expressions.Add(firstEx);
                            while (LA(Comma))
                            {
                                Step();
                                ae.Expressions.Add(AssignExpression());
                            }
                        }
                        else
                            SynErr(CloseSquareBracket);
                    }

                    Expect(CloseSquareBracket);
                }

                else if (LA(Dot))
                {
                    Step();
                    AssignTokenExpression ae = new AssignTokenExpression(Dot);
                    ae.PrevExpression = retEx;
                    if (LA(New))
                        ae.FollowingExpression = NewExpression();
                    else if (LA(Identifier))
                    {
                        Step();
                        ae.FollowingExpression = new IdentExpression(t.Value);
                    }
                    else
                        SynErr(Identifier, "Identifier or new expected");

                    retEx = ae;
                }
                else if (LA(Increment) || LA(Decrement))
                {
                    Step();
                    DExpression ex2 = new TokenExpression(t.Kind);
                    ex2.Base = retEx;
                    retEx = ex2;
                }
            }

            return retEx;
        }

        DExpression PrimaryExpression()
        {
            if (LA(Identifier) && PK(Not))
                return new TypeDeclarationExpression(TemplateInstance());

            if (LA(Identifier))
            {
                Step();
                return new IdentExpression(t.Value);
            }

            if (LA(This) || LA(Super) || LA(Null) || LA(True) || LA(False) || LA(Dollar))
            {
                Step();
                return new TokenExpression(t.Kind);
            }

            if (LA(Literal))
            {
                Step();
                return new IdentExpression(t.LiteralValue);
            }

            if (LA(Dot))
            {
                Step();
                Expect(Identifier);
                DExpression ret = new IdentExpression(t.Value);
                ret.Base = new TokenExpression(Dot);
                return ret;
            }

            // ArrayLiteral
            // AssocArrayLiteral
            if (LA(OpenSquareBracket))
            {
                Step();
                ArrayExpression arre = new ArrayExpression();

                DExpression firstCondExpr = ConditionalExpression();
                // Can be an associative array only
                if (LA(Colon))
                {
                    Step();
                    AssignTokenExpression ae = new AssignTokenExpression(Colon);
                    ae.PrevExpression = firstCondExpr;
                    ae.FollowingExpression = ConditionalExpression();
                    arre.Expressions.Add(ae);

                    while (LA(Comma))
                    {
                        Step();
                        ae = new AssignTokenExpression(Colon);
                        ae.PrevExpression = ConditionalExpression();
                        Expect(Colon);
                        ae.FollowingExpression = ConditionalExpression();
                        arre.Expressions.Add(ae);
                    }
                }
                else
                {
                    if (AssignOps[la.Kind])
                    {
                        Step();
                        AssignTokenExpression ae = new AssignTokenExpression(t.Kind);
                        ae.PrevExpression = firstCondExpr;
                        ae.FollowingExpression = AssignExpression();
                        arre.Expressions.Add(ae);
                    }

                    while (LA(Comma))
                    {
                        Step();
                        arre.Expressions.Add(AssignExpression());
                    }
                }

                Expect(CloseSquareBracket);
                return arre;
            }

            //TODO: FunctionLiteral

            // AssertExpression
            if (LA(Assert))
            {
                Step();
                Expect(OpenParenthesis);
                ClampExpression ce = new ClampExpression(ClampExpression.ClampType.Round);
                ce.FrontExpression = new TokenExpression(Assert);
                ce.InnerExpression = AssignExpression();

                if (LA(Comma))
                {
                    Step();
                    AssignTokenExpression ate = new AssignTokenExpression(Comma);
                    ate.PrevExpression = ce.InnerExpression;
                    ate.FollowingExpression = AssignExpression();
                    ce.InnerExpression = ate;
                }
                Expect(CloseParenthesis);
                return ce;
            }

            // MixinExpression | ImportExpression
            if (LA(Mixin) || LA(Import))
            {
                Step();
                int tk = t.Kind;
                Expect(OpenParenthesis);
                ClampExpression ce = new ClampExpression(ClampExpression.ClampType.Round);
                ce.FrontExpression = new TokenExpression(tk);
                ce.InnerExpression = AssignExpression();
                Expect(CloseParenthesis);
                return ce;
            }

            // Typeof
            if (LA(Typeof))
            {
                return new TypeDeclarationExpression(TypeOf());
            }

            // TypeidExpression
            if (LA(Typeid))
            {

            }
            // IsExpression
            if (LA(Is))
            {

            }
            // ( Expression )
            if (LA(OpenParenthesis))
            {
                Step();
                DExpression ret = Expression();
                Expect(CloseParenthesis);
                return ret;
            }
            // TraitsExpression
            if (LA(__traits))
            {
                return TraitsExpression();
            }

            // BasicType . Identifier
            if (LA(Const) || LA(Immutable) || LA(Shared) || LA(InOut))
            {
                Step();
                int tk = t.Kind;
                Expect(OpenParenthesis);
                ClampExpression ce = new ClampExpression(ClampExpression.ClampType.Round);
                ce.FrontExpression = new TokenExpression(tk);
                ce.InnerExpression = new TypeDeclarationExpression(Type());
                Expect(CloseParenthesis);

                Expect(Dot);
                Expect(Identifier);
                AssignTokenExpression ate = new AssignTokenExpression(Dot);
                ate.PrevExpression = ce;
                ate.FollowingExpression = new IdentExpression(t.Value);
            }

            SynErr(t.Kind, "Identifier expected when parsing an expression");
            return null;
        }
        #endregion

        #region Statements
        void Statement(ref DNode par, bool CanBeEmpty, bool BlocksAllowed)
        {
            if (CanBeEmpty && LA(Semicolon))
            {
                Step();
                return;
            }

            else if (BlocksAllowed && LA(OpenCurlyBrace))
            {
                BlockStatement(ref par);
                return;
            }

            // LabeledStatement
            else if (LA(Identifier) && PK(Colon))
            {
                Step();
                Step();
                return;
            }

            // IfStatement
            else if (LA(If))
            {
                Step();
                Expect(OpenParenthesis);

                // IfCondition
                if (LA(Auto))
                {
                    Step();
                    Expect(Identifier);
                    Expect(Assign);
                    Expression();
                }
                else if (IsAssignExpression())
                    Expression();
                else
                {
                    Declarator(false);
                    Expect(Assign);
                    Expression();
                }

                Expect(CloseParenthesis);
                // ThenStatement
                Statement(ref par, false, true);

                // ElseStatement
                if (LA(Else))
                {
                    Step();
                    Statement(ref par, false, true);
                }
            }

            // WhileStatement
            else if (LA(While))
            {
                Step();
                Expect(OpenParenthesis);
                Expression();
                Expect(CloseParenthesis);

                Statement(ref par, false, true);
            }

            // DoStatement
            else if (LA(Do))
            {
                Step();
                Statement(ref par, false, true);
                Expect(While);
                Expect(OpenParenthesis);
                Expression();
                Expect(CloseParenthesis);
            }

            // ForStatement
            else if (LA(For))
            {
                Step();
                Expect(OpenParenthesis);

                // Initialize
                if (LA(Semicolon))
                    Step();
                else
                    Statement(ref par, false, true);

                // Test
                if (!LA(Semicolon))
                    Expression();

                Expect(Semicolon);

                // Increment
                if (!LA(CloseParenthesis))
                    Expression();

                Expect(CloseParenthesis);

                Statement(ref par, false, true);
            }

            // ForeachStatement
            else if (LA(Foreach) || LA(Foreach_Reverse))
            {
                Step();
                Expect(OpenParenthesis);

                bool init = true;
                while (init || LA(Comma))
                {
                    if (!init) Step();
                    init = false;

                    if (LA(Ref))
                        Step();

                    if (LA(Identifier) && (PK(Semicolon) || lexer.CurrentPeekToken.Kind == Comma))
                    {
                        Step();
                    }
                    else
                    {
                        Type();
                        Expect(Identifier);
                    }
                }

                Expect(Semicolon);
                Expression();

                // ForeachRangeStatement
                if (LA(Dot) && PK(Dot))
                {
                    Step();
                    Step();
                    Expression();
                }

                Expect(CloseParenthesis);

                Statement(ref par, false, true);
            }

            // [Final] SwitchStatement
            else if ((LA(Final) && PK(Switch)) || LA(Switch))
            {
                if (LA(Final))
                    Step();
                Step();
                Expect(OpenParenthesis);
                Expression();
                Expect(CloseParenthesis);
                Statement(ref par, false, true);
            }

            // CaseStatement
            else if (LA(Case))
            {
                Step();

                AssignExpression();

                if (!(LA(Colon) && PK(Dot) && Peek().Kind == Dot))
                    while (LA(Comma))
                        AssignExpression();

                Expect(Colon);

                // CaseRangeStatement
                if (LA(Dot) && PK(Dot))
                {
                    Step();
                    Step();
                    Expect(Case);
                    AssignExpression();
                    Expect(Colon);

                    Statement(ref par, true, true);
                }
            }

            // Default
            else if (LA(Default))
            {
                Step();
                Expect(Colon);
                Statement(ref par, true, true);
            }

            // Continue | Break
            else if (LA(Continue) || LA(Break))
            {
                Step();
                if (LA(Identifier))
                    Step();
                Expect(Semicolon);
            }

            // Return
            else if (LA(Return))
            {
                Step();
                if (!LA(Semicolon))
                    Expression();
                Expect(Semicolon);
            }

            // Goto
            else if (LA(Goto))
            {
                Step();
                if (LA(Identifier) || LA(Default))
                {
                    Step();
                }
                else if (LA(Case))
                {
                    Step();
                    if (!LA(Semicolon))
                        Expression();
                }

                Expect(Semicolon);
            }

            // WithStatement
            else if (LA(With))
            {
                Step();
                Expect(OpenParenthesis);

                // Symbol
                if (LA(Identifier))
                    IdentifierList();
                else
                    Expression();

                Expect(CloseParenthesis);
                Statement(ref par, false, true);
            }

            // SynchronizedStatement
            else if (LA(Synchronized))
            {
                Step();
                if (LA(OpenParenthesis))
                {
                    Step();
                    Expression();
                    Expect(CloseParenthesis);
                }
                Statement(ref par, false, true);
            }

            // TryStatement
            else if (LA(Try))
            {
                Step();
                Statement(ref par, false, true);

                if (!(LA(Catch) || LA(Finally)))
                    SynErr(Catch, "catch or finally expected");

                // Catches
            do_catch:
                if (LA(Catch))
                {
                    Step();

                    // CatchParameter
                    if (LA(OpenParenthesis))
                    {
                        DVariable catchVar = new DVariable();
                        catchVar.Type = BasicType();
                        Expect(Identifier);
                        catchVar.name = t.Value;
                        Expect(CloseParenthesis);

                        Statement(ref par, false, true);

                        if (LA(Catch))
                            goto do_catch;
                    }
                }

                if (LA(Finally))
                {
                    Step();

                    Statement(ref par, false, true);
                }
            }

            // ThrowStatement
            else if (LA(Throw))
            {
                Step();

                Expression();
            }

            // ScopeGuardStatement
            else if (LA(Scope))
            {
                Step();
                Expect(OpenParenthesis);
                Expect(Identifier); // exit, failure, success
                Expect(CloseParenthesis);
                Statement(ref par, false, true);
            }

            // AsmStatement
            else if (LA(Asm))
            {
                Step();
                Expect(OpenCurlyBrace);

                while (!IsEOF && !LA(CloseCurlyBrace))
                {
                    Step();
                }

                Expect(CloseCurlyBrace);
            }

            // PragmaStatement
            else if (LA(Pragma))
            {
                _Pragma();
                Statement(ref par, true, true);
            }

            // MixinStatement
            else if (LA(Mixin))
            {
                Step();
                Expect(OpenParenthesis);

                AssignExpression();

                Expect(CloseParenthesis);
                Expect(Semicolon);
            }

            else if (IsAssignExpression())
                AssignExpression();

            else Declaration(ref par);
        }

        void BlockStatement(ref DNode par)
        {
            Expect(OpenCurlyBrace);

            while (!IsEOF && !LA(CloseCurlyBrace))
            {
                Statement(ref par, true, true);
            }

            Expect(CloseCurlyBrace);
        }
        #endregion

        #region Structs & Unions
        private void AggregateDeclaration()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Classes
        private void ClassDeclaration()
        {
            throw new NotImplementedException();
        }

        private List<TypeDeclaration> BaseClassList()
        {
            throw new NotImplementedException();
        }

        void Constructor(ref DNode par)
        {

        }

        void Destructor(ref DNode par)
        {

        }
        #endregion

        #region Interfaces
        private void InterfaceDeclaration()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Enums
        private void EnumDeclaration()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Functions
        void FunctionBody(ref DNode par)
        {
            bool HadIn = false, HadOut = false;

        check_again:
            if (!HadIn && LA(In))
            {
                HadIn = true;
                Step();
                BlockStatement(ref par);

                if (!HadOut && LA(Out))
                    goto check_again;
            }

            if (!HadOut && LA(Out))
            {
                HadOut = true;
                Step();

                if (LA(OpenParenthesis))
                {
                    Step();
                    Expect(Identifier);
                    Expect(CloseParenthesis);
                }

                BlockStatement(ref par);

                if (!HadIn && LA(In))
                    goto check_again;
            }

            if (HadIn || HadOut)
                Expect(Body);
            else if (LA(Body))
                Step();

            BlockStatement(ref par);

        }
        #endregion

        #region Templates

        /// <summary>
        /// Be a bit lazy here with checking whether there're templates or not
        /// </summary>
        private bool IsTemplateParameterList()
        {
            lexer.StartPeek();
            int r = 0;
            while (r >= 0 && lexer.CurrentPeekToken.Kind != EOF)
            {
                if (lexer.CurrentPeekToken.Kind == OpenParenthesis) r++;
                else if (lexer.CurrentPeekToken.Kind == CloseParenthesis)
                {
                    r--;
                    if (r < 0 && Peek().Kind == OpenParenthesis)
                        return true;
                }
                Peek();
            }
            return false;
        }

        private List<DNode> TemplateParameterList()
        {
            throw new NotImplementedException();
        }

        private TypeDeclaration TemplateInstance()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Traits
        DExpression TraitsExpression()
        {
            Expect(__traits);
            Expect(OpenParenthesis);
            ClampExpression ce = new ClampExpression(new TokenExpression(__traits), ClampExpression.ClampType.Round);

            //TODO: traits keywords

            Expect(CloseParenthesis);
            return ce;
        }
        #endregion
    }

}