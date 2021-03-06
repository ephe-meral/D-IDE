﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.239
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace D_IDE.D {
    using System;
    
    
    /// <summary>
    ///   Eine stark typisierte Ressourcenklasse zum Suchen von lokalisierten Zeichenfolgen usw.
    /// </summary>
    // Diese Klasse wurde von der StronglyTypedResourceBuilder automatisch generiert
    // -Klasse über ein Tool wie ResGen oder Visual Studio automatisch generiert.
    // Um einen Member hinzuzufügen oder zu entfernen, bearbeiten Sie die .ResX-Datei und führen dann ResGen
    // mit der /str-Option erneut aus, oder Sie erstellen Ihr VS-Projekt neu.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class DResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DResources() {
        }
        
        /// <summary>
        ///   Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("D_IDE.D.DResources", typeof(DResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Überschreibt die CurrentUICulture-Eigenschaft des aktuellen Threads für alle
        ///   Ressourcenzuordnungen, die diese stark typisierte Ressourcenklasse verwenden.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static byte[] D_xshd {
            get {
                object obj = ResourceManager.GetObject("D_xshd", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die 
        ///// To enable enhanced visual styles for Win32 Apps, link in a manifest file
        ///1 24 &quot;Resources\Win32Manifest.manifest&quot; ähnelt.
        /// </summary>
        internal static string defResource {
            get {
                return ResourceManager.GetString("defResource", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die import std.stdio, std.cstream;
        ///
        ///void main(string[] args)
        ///{
        ///	// Prints &quot;Hello World&quot; string in console
        ///	writeln(\&quot;Hello World!\&quot;);
        ///
        ///	// Lets the user press &lt;Return&gt; before program stops
        ///	din.getc();
        ///}
        /// ähnelt.
        /// </summary>
        internal static string helloWorldConsoleApp {
            get {
                return ResourceManager.GetString("helloWorldConsoleApp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die 
        ///
        ///class MyClass
        ///{
        ///	//TODO: Enter class code here
        ///}
        /// ähnelt.
        /// </summary>
        internal static string libExample {
            get {
                return ResourceManager.GetString("libExample", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die &lt;?xml version=&quot;1.0&quot; encoding=&quot;UTF-8&quot; standalone=&quot;yes&quot;?&gt;
        ///&lt;assembly xmlns=&quot;urn:schemas-microsoft-com:asm.v1&quot; manifestVersion=&quot;1.0&quot;&gt;
        ///&lt;assemblyIdentity name=&quot;DApplication&quot; processorArchitecture=&quot;x86&quot; version=&quot;1.0.0.0&quot; type=&quot;win32&quot;/&gt;
        ///&lt;description&gt;&lt;/description&gt;
        ///&lt;dependency&gt;
        ///&lt;dependentAssembly&gt;
        ///&lt;assemblyIdentity type=&quot;win32&quot; name=&quot;Microsoft.Windows.Common-Controls&quot; version=&quot;6.0.0.0&quot; processorArchitecture=&quot;x86&quot; publicKeyToken=&quot;6595b64144ccf1df&quot; language=&quot;*&quot; /&gt;
        ///&lt;/dependentAssembly&gt;
        ///&lt;/dependency&gt;
        ///&lt;/assembly&gt;
        /// ähnelt.
        /// </summary>
        internal static string Win32Manifest {
            get {
                return ResourceManager.GetString("Win32Manifest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die 
        ///// Example code taken from winsamp.d (Listed under D examples)
        ///
        ///pragma(lib, &quot;gdi32.lib&quot;);
        ///import core.runtime;
        ///import std.c.windows.windows;
        ///import std.string;
        ///
        ///enum IDC_BTNCLICK     = 101;
        ///enum IDC_BTNDONTCLICK = 102;
        ///
        ///extern(Windows)
        ///int WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int iCmdShow)
        ///{
        ///    int result;
        ///    void exceptionHandler(Throwable e) { throw e; }
        ///
        ///    try
        ///    {
        ///        Runtime.initialize(&amp;exceptionHandler);
        ///        result = myWinMain(hInstance, hPrevInstance, l [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        internal static string winsamp_d {
            get {
                return ResourceManager.GetString("winsamp_d", resourceCulture);
            }
        }
    }
}
