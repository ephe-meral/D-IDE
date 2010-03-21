
#pragma once
#include "stdafx.h"

using namespace System;

namespace DebugEngineWrapper
{
	public enum class DebugEvent
	{
		/*
		DEBUG_EVENT_BREAKPOINT Breakpoint A breakpoint exception occurred in the target. 
		DEBUG_EVENT_EXCEPTION Exception An exception debugging event occurred in the target. 
		DEBUG_EVENT_CREATE_THREAD CreateThread A create-thread debugging event occurred in the target. 
		DEBUG_EVENT_EXIT_THREAD ExitThread An exit-thread debugging event occurred in the target. 
		DEBUG_EVENT_CREATE_PROCESS CreateProcess A create-process debugging event occurred in the target. 
		DEBUG_EVENT_EXIT_PROCESS ExitProcess An exit-process debugging event occurred in the target. 
		DEBUG_EVENT_LOAD_MODULE LoadModule A module-load debugging event occurred in the target. 
		DEBUG_EVENT_UNLOAD_MODULE UnloadModule A module-unload debugging event occurred in the target. 
		DEBUG_EVENT_SYSTEM_ERROR SystemError A system error occurred in the target. 


		The following events are generated by the debugger engine.
		DEBUG_EVENT_SESSION_STATUS SessionStatus A change has occurred in the session status. 
		DEBUG_EVENT_CHANGE_DEBUGGEE_STATE ChangeDebuggeeState The engine has made or detected a change in the target status. 
		DEBUG_EVENT_CHANGE_ENGINE_STATE ChangeEngineState The engine state has changed. 
		DEBUG_EVENT_CHANGE_SYMBOL_STATE ChangeSymbolState The symbol state has changed. 

		*/
		Breakpoint = DEBUG_EVENT_BREAKPOINT,
		Exception = DEBUG_EVENT_EXCEPTION,
		CreateThread = DEBUG_EVENT_CREATE_THREAD,
		ExitThread = DEBUG_EVENT_EXIT_THREAD,
		CreateProcess = DEBUG_EVENT_CREATE_PROCESS,
		ExitProcess = DEBUG_EVENT_EXIT_PROCESS,
		LoadModule = DEBUG_EVENT_LOAD_MODULE,
		UnloadModule = DEBUG_EVENT_UNLOAD_MODULE,
		SystemError = DEBUG_EVENT_SYSTEM_ERROR,

		SessionStatus = DEBUG_EVENT_SESSION_STATUS,
		ChangeDebuggeeState = DEBUG_EVENT_CHANGE_DEBUGGEE_STATE,
		ChangeEngineState = DEBUG_EVENT_CHANGE_ENGINE_STATE,
		ChangeSymbolState = DEBUG_EVENT_CHANGE_SYMBOL_STATE
	};

	public enum class CreateFlags
	{
		DebugProcess			= DEBUG_PROCESS, //                       0x00000001
		DebugOnlyThisProcess	= DEBUG_ONLY_THIS_PROCESS, //             0x00000002
		CreateSuspended			= CREATE_SUSPENDED, //                    0x00000004
		CreateNewConsole		= CREATE_NEW_CONSOLE, //                  0x00000010
		StackSizeParamIsAReservation = STACK_SIZE_PARAM_IS_A_RESERVATION, //0x00010000
		InheritCallerPriority	= INHERIT_CALLER_PRIORITY, //             0x00020000

		// On Windows XP this flag prevents the debug
		// heap from being used in the new process.
		NoDebugHeap				= DEBUG_CREATE_PROCESS_NO_DEBUG_HEAP,	//    CREATE_UNICODE_ENVIRONMENT
		// Indicates that the native NT RTL process creation
		// routines should be used instead of Win32.  This
		// is only meaningful for special processes that run
		// as NT native processes.
		ProcessThroughRTL		= DEBUG_CREATE_PROCESS_THROUGH_RTL		//    STACK_SIZE_PARAM_IS_A_RESERVATION
	};

	public enum class EngCreateFlags
	{
		Default					= DEBUG_ECREATE_PROCESS_DEFAULT,			//    0x00000000
		InheritHandles			= DEBUG_ECREATE_PROCESS_INHERIT_HANDLES,	//    0x00000001
		UseVerifierFlags		= DEBUG_ECREATE_PROCESS_USE_VERIFIER_FLAGS, //    0x00000002
		UseImplicitCommandLine	= DEBUG_ECREATE_PROCESS_USE_IMPLICIT_COMMAND_LINE // 0x00000004
	};

	public value class DebugCreateProcessOptions
	{
	public:
		CreateFlags CreateFlags;
		EngCreateFlags EngCreateFlags;
		ULONG  VerifierFlags;
		ULONG  Reserved;

	internal:
		DEBUG_CREATE_PROCESS_OPTIONS ToLegacy()
		{
			DEBUG_CREATE_PROCESS_OPTIONS result;
			result.CreateFlags = (ULONG)this->CreateFlags;
			result.EngCreateFlags = (ULONG)this->EngCreateFlags;
			result.VerifierFlags = this->VerifierFlags;
			result.Reserved = this->Reserved;
			return result;
		}
	};

	private enum class ModuleNameType
	{
		Image			= DEBUG_MODNAME_IMAGE,
		Module			= DEBUG_MODNAME_MODULE,
		LoadedImage		= DEBUG_MODNAME_LOADED_IMAGE,
		SymbolFile		= DEBUG_MODNAME_SYMBOL_FILE,
		MappedImage		= DEBUG_MODNAME_MAPPED_IMAGE
	};

	public enum class OutputFlags
	{
		// Output mask bits.
		// Normal output.
		Normal			= DEBUG_OUTPUT_NORMAL, //            0x00000001
		// Error output.
		Error			= DEBUG_OUTPUT_ERROR, //             0x00000002
		// Warnings.
		Warning			= DEBUG_OUTPUT_WARNING, //           0x00000004
		// Additional output.
		Verbose			= DEBUG_OUTPUT_VERBOSE, //           0x00000008
		// Prompt output.
		Prompt			= DEBUG_OUTPUT_PROMPT, //            0x00000010
		// Register dump before prompt.
		PromptRegisters = DEBUG_OUTPUT_PROMPT_REGISTERS, //  0x00000020
		// Warnings specific to extension operation.
		ExtensionWarning = DEBUG_OUTPUT_EXTENSION_WARNING, //0x00000040
		// Debuggee debug output, such as from OutputDebugString.
		Debuggee		= DEBUG_OUTPUT_DEBUGGEE, //          0x00000080
		// Debuggee-generated prompt, such as from DbgPrompt.
		DebuggeePrompt	= DEBUG_OUTPUT_DEBUGGEE_PROMPT, //   0x00000100
		// Symbol messages, such as for !sym noisy.
		Symbols			= DEBUG_OUTPUT_SYMBOLS  //           0x00000200

		////BreakpointOutput = 0x20000000,
		//Debuggee_		= DEBUG_OUTPUT_DEBUGGEE,
		//DebuggeePrompt	= DEBUG_OUTPUT_DEBUGGEE_PROMPT,
		//Error			= DEBUG_OUTPUT_ERROR,
		////EventOutput = 0x10000000,
		//ExtensionWarning = DEBUG_OUTPUT_EXTENSION_WARNING,
		////KdProtocolOutput = -2147483648,
		//Normal			= 1,
		//Prompt			= DEBUG_OUTPUT_PROMPT,
		//PromptRegisters = DEBUG_OUTPUT_PROMPT_REGISTERS,
		////RemotingOutput = 0x40000000,
		//Symbols			= DEBUG_OUTPUT_SYMBOLS,
		//Verbose			= DEBUG_OUTPUT_VERBOSE,
		//Warning			= DEBUG_OUTPUT_WARNING
	};

	public enum class DbgExpressionType
	{
		MASM			= DEBUG_EXPR_MASM,
		CPP				= DEBUG_EXPR_CPLUSPLUS
	};

	public enum class DbgValueType
	{
		Invalid			= DEBUG_VALUE_INVALID,
		Integer8		= DEBUG_VALUE_INT8,
		Integer16		= DEBUG_VALUE_INT16,
		Integer32		= DEBUG_VALUE_INT32,
		Integer64		= DEBUG_VALUE_INT64,
		Float32			= DEBUG_VALUE_FLOAT32,
		Float64			= DEBUG_VALUE_FLOAT64,
		Float80			= DEBUG_VALUE_FLOAT80,
		Float82			= DEBUG_VALUE_FLOAT82,
		Float128		= DEBUG_VALUE_FLOAT128,
		Vector64		= DEBUG_VALUE_VECTOR64,
		Vector128		= DEBUG_VALUE_VECTOR128
	};

	public enum class SymbolMode
	{
		NoType			= 0,
		Coff			= 1,
		CodeView		= 2,
		Pdb				= 3,
		Export			= 4,
		Deferred		= 5,
		Sym				= 6,
		Dia				= 7
	};

	public enum class ModuleFlags
	{
		Unloaded		= DEBUG_MODULE_UNLOADED,
		UserMode		= DEBUG_MODULE_USER_MODE,
		BadChecksum		= DEBUG_MODULE_SYM_BAD_CHECKSUM
	};

	public enum class DumpType
	{
		/*
		DEBUG_DUMP_SMALL Small Memory Dump (kernel-mode) or Minidump (user-mode). 
		DEBUG_DUMP_DEFAULT Full User-Mode Dump (user-mode) or Kernel Summary Dump (kernel-mode). 
		DEBUG_DUMP_FULL (kernel-mode only) Complete Memory Dump. 

		Moreover, the following aliases are available for kernel-mode debugging.

		DEBUG_KERNEL_SMALL_DUMP DEBUG_DUMP_SMALL 
		DEBUG_KERNEL_DUMP DEBUG_DUMP_DEFAULT 
		DEBUG_KERNEL_FULL_DUMP DEBUG_DUMP_FULL 

		Additionally, the following aliases are available for user-mode debugging.

		DEBUG_USER_WINDOWS_SMALL_DUMP DEBUG_DUMP_SMALL 
		DEBUG_USER_WINDOWS_DUMP DEBUG_DUMP_DEFAULT 
		*/
		Default		= DEBUG_DUMP_DEFAULT,
		Small		= DEBUG_DUMP_SMALL,
		Full		= DEBUG_DUMP_FULL
	};

	public enum class DumpFlags
	{
		/*
		DEBUG_FORMAT_USER_SMALL_FULL_MEMORY Add full memory data. All accessible committed pages owned by the target application will be included. 
		DEBUG_FORMAT_USER_SMALL_HANDLE_DATA Add data about the handles that are associated with the target application. 
		DEBUG_FORMAT_USER_SMALL_UNLOADED_MODULES Add unloaded module information. This information is available only in Windows Server 2003 and later versions of Windows. 
		DEBUG_FORMAT_USER_SMALL_INDIRECT_MEMORY Add indirect memory. A small region of memory that surrounds any address that is referenced by a pointer on the stack or backing store is included. 
		DEBUG_FORMAT_USER_SMALL_DATA_SEGMENTS Add all data segments within the executable images. 
		DEBUG_FORMAT_USER_SMALL_FILTER_MEMORY Set to zero all of the memory on the stack and in the backing store that is not useful for recreating the stack trace. This can make compression of the Minidump more efficient and increase privacy by removing unnecessary information. 
		DEBUG_FORMAT_USER_SMALL_FILTER_PATHS Remove the module paths, leaving only the module names. This is useful for protecting privacy by hiding the directory structure (which may contain the user�s name). 
		DEBUG_FORMAT_USER_SMALL_PROCESS_THREAD_DATA Add the process environment block (PEB) and thread environment block (TEB). This flag can be used to provide Windows system information for threads and processes. 
		DEBUG_FORMAT_USER_SMALL_PRIVATE_READ_WRITE_MEMORY Add all committed private read-write memory pages. 
		DEBUG_FORMAT_USER_SMALL_NO_OPTIONAL_DATA Prevent privacy-sensitive data from being included in the Minidump. Currently, this flag excludes from the Minidump data that would have been added due to the following flags being set: DEBUG_FORMAT_USER_SMALL_PROCESS_THREAD_DATA, DEBUG_FORMAT_USER_SMALL_FULL_MEMORY, DEBUG_FORMAT_USER_SMALL_INDIRECT_MEMORY, DEBUG_FORMAT_USER_SMALL_PRIVATE_READ_WRITE_MEMORY. 
		DEBUG_FORMAT_USER_SMALL_FULL_MEMORY_INFO Add all basic memory information. This is the information returned by the QueryVirtual method. The information for all memory is included, not just valid memory, which allows the debugger to reconstruct the complete virtual memory layout from the Minidump. 
		DEBUG_FORMAT_USER_SMALL_THREAD_INFO Add additional thread information, which includes execution time, start time, exit time, start address, and exit status. 
		DEBUG_FORMAT_USER_SMALL_CODE_SEGMENTS Add all code segments with the executable images. 
		*/
		FullMemory			= DEBUG_FORMAT_USER_SMALL_FULL_MEMORY,
		HandleData			= DEBUG_FORMAT_USER_SMALL_HANDLE_DATA,
		UnloadedModules		= DEBUG_FORMAT_USER_SMALL_UNLOADED_MODULES,
		IndirectMemory		= DEBUG_FORMAT_USER_SMALL_INDIRECT_MEMORY,
		DataSegments		= DEBUG_FORMAT_USER_SMALL_DATA_SEGMENTS,
		FilterMemory		= DEBUG_FORMAT_USER_SMALL_FILTER_MEMORY,
		FilterPaths			= DEBUG_FORMAT_USER_SMALL_FILTER_PATHS,
		ThreadData			= DEBUG_FORMAT_USER_SMALL_PROCESS_THREAD_DATA,
		PrivateRWMemroy		= DEBUG_FORMAT_USER_SMALL_PRIVATE_READ_WRITE_MEMORY,
		NoOptionalData		= DEBUG_FORMAT_USER_SMALL_NO_OPTIONAL_DATA,
		FullMemoryInfo		= DEBUG_FORMAT_USER_SMALL_FULL_MEMORY_INFO,
		ThreadInfo			= DEBUG_FORMAT_USER_SMALL_THREAD_INFO,
		CodeSegments		= DEBUG_FORMAT_USER_SMALL_CODE_SEGMENTS
	};

	public enum class AttachFlags
	{
		/*
		DEBUG_ATTACH_NONINVASIVE Attach to the target noninvasively. For more information about noninvasive debugging, see Noninvasive Debugging (User Mode).
		If this flag is set, then the flags DEBUG_ATTACH_EXISTING, DEBUG_ATTACH_INVASIVE_NO_INITIAL_BREAK, and DEBUG_ATTACH_INVASIVE_RESUME_PROCESS must not be set.

		DEBUG_ATTACH_EXISTING Re-attach to an application to which a debugger has already attached (and possibly abandoned). For more information about re-attaching to targets, see Re-attaching to the Target Application.
		If this flag is set, then the other DEBUG_ATTACH_XXX flags must not be set.

		DEBUG_ATTACH_NONINVASIVE_NO_SUSPEND Do not suspend the target's threads when attaching noninvasively.
		If this flag is set, then the flag DEBUG_ATTACH_NONINVASIVE must also be set.

		DEBUG_ATTACH_INVASIVE_NO_INITIAL_BREAK (Windows XP and later) Do not request an initial break-in when attaching to the target.
		If this flag is set, then the flags DEBUG_ATTACH_NONINVASIVE and DEBUG_ATTACH_EXISTING must not be set.

		DEBUG_ATTACH_INVASIVE_RESUME_PROCESS Resume all of the target's threads when attaching invasively.
		If this flag is set, then the flags DEBUG_ATTACH_NONINVASIVE and DEBUG_ATTACH_EXISTING must not be set.
		*/
		NonInvasive				= DEBUG_ATTACH_NONINVASIVE,
		Existing				= DEBUG_ATTACH_EXISTING,
		NonInvasiveNoSuspend	= DEBUG_ATTACH_NONINVASIVE_NO_SUSPEND,
		InvasiveNoInitialBreak	= DEBUG_ATTACH_INVASIVE_NO_INITIAL_BREAK,
		InvasiveResumeProcess	= DEBUG_ATTACH_INVASIVE_RESUME_PROCESS
	};
	public enum class DebugStatus
	{
		NoChange				= DEBUG_STATUS_NO_CHANGE,	//           0
		Go						= DEBUG_STATUS_GO,			//           1
		GoHandled				= DEBUG_STATUS_GO_HANDLED,	//           2
		GoNotHandled			= DEBUG_STATUS_GO_NOT_HANDLED, //        3
		StepOver				= DEBUG_STATUS_STEP_OVER,	//           4
		StepInto				= DEBUG_STATUS_STEP_INTO,	//           5
		Break					= DEBUG_STATUS_BREAK,		//           6
		NoDebuggee				= DEBUG_STATUS_NO_DEBUGGEE,	//           7
		StopBranch				= DEBUG_STATUS_STEP_BRANCH,	//           8
		IgnoreEvent				= DEBUG_STATUS_IGNORE_EVENT, //          9
		RestartRequested		= DEBUG_STATUS_RESTART_REQUESTED, //     10
		ReverseGo				= DEBUG_STATUS_REVERSE_GO,	//           11
		ReverseStepBranch		= DEBUG_STATUS_REVERSE_STEP_BRANCH, //   12
		ReverseStepOver			= DEBUG_STATUS_REVERSE_STEP_OVER, //     13
		ReverseStepInto			= DEBUG_STATUS_REVERSE_STEP_INTO, //     14
	};
	public enum class DebugOutputControl
	{
		// Output control flags.
		// Output generated by methods called by this
		// client will be sent only to this clients
		// output callbacks.
		ThisClient				= DEBUG_OUTCTL_THIS_CLIENT,	//       0x00000000
		// Output will be sent to all clients.
		AllClients				= DEBUG_OUTCTL_ALL_CLIENTS,	//       0x00000001
		// Output will be sent to all clients except
		// the client generating the output.
		AllOtherClients			= DEBUG_OUTCTL_ALL_OTHER_CLIENTS, //  0x00000002
		// Output will be discarded immediately and will not
		// be logged or sent to callbacks.
		Ignore					= DEBUG_OUTCTL_IGNORE,		//       0x00000003
		// Output will be logged but not sent to callbacks.
		LogOnly					= DEBUG_OUTCTL_LOG_ONLY,	//       0x00000004
		// All send control bits.
		SendMask				= DEBUG_OUTCTL_SEND_MASK,	//       0x00000007
		// Do not place output from this client in
		// the global log file.
		NotLogged				= DEBUG_OUTCTL_NOT_LOGGED,	//       0x00000008
		// Send output to clients regardless of whether the
		// mask allows it or not.
		OverrideMask			= DEBUG_OUTCTL_OVERRIDE_MASK, //     0x00000010
		// Text is markup instead of plain text.
		//				DML						= DEBUG_OUTCTL_DML,			//       0x00000020

		// Special values which mean leave the output settings
		// unchanged.
		//				AmbientDML				= DEBUG_OUTCTL_AMBIENT_DML,	//       0xfffffffe
		//				AmbientTest				= DEBUG_OUTCTL_AMBIENT_TEXT, //      0xffffffff

		// Old ambient flag which maps to text.
		Ambient					= DEBUG_OUTCTL_AMBIENT		//       DEBUG_OUTCTL_AMBIENT_TEXT
	};
	public enum class DebugExecuteFlags
	{
		// Execute and ExecuteCommandFile flags.
		// These flags only apply to the command
		// text itself; output from the executed
		// command is controlled by the output
		// control parameter.
		// Default execution.  Command is logged
		// but not output.
		Default					= DEBUG_EXECUTE_DEFAULT,	//       0x00000000
		// Echo commands during execution.  In
		// ExecuteCommandFile also echoes the prompt
		// for each line of the file.
		Echo					= DEBUG_EXECUTE_ECHO,		//       0x00000001
		// Do not log or output commands during execution.
		// Overridden by DEBUG_EXECUTE_ECHO.
		NotLogged				= DEBUG_EXECUTE_NOT_LOGGED,	//       0x00000002
		// If this flag is not set an empty string
		// to Execute will repeat the last Execute
		// string.
		NoRepeat				= DEBUG_EXECUTE_NO_REPEAT,	//       0x00000004
	};
	public enum class TypeOptions
	{
		//
		// Type options, used with Get/SetTypeOptions.
		//
		// Display PUSHORT and USHORT arrays in Unicode.
		UnicodeDisplay			= DEBUG_TYPEOPTS_UNICODE_DISPLAY, //    0x00000001
		// Display LONG types in default base instead of decimal.
		LongStatusDisplay		= DEBUG_TYPEOPTS_LONGSTATUS_DISPLAY, // 0x00000002
		// Display integer types in default base instead of decimal.
		ForceRadisOutput		= DEBUG_TYPEOPTS_FORCERADIX_OUTPUT, //  0x00000004
		// Search for the type/symbol with largest size when
		// multiple type/symbol match for a given name
		MatchMaxsize			= DEBUG_TYPEOPTS_MATCH_MAXSIZE  //      0x00000008
	};
	public enum class GetMethodFlags
	{
		// Scan all modules, loaded and unloaded.
		Default					= DEBUG_GETMOD_DEFAULT,        //     0x00000000
		// Do not scan loaded modules.
		NoLoadedModules			= DEBUG_GETMOD_NO_LOADED_MODULES, //   0x00000001
		// Do not scan unloaded modules.
		NoUloadedModules		= DEBUG_GETMOD_NO_UNLOADED_MODULES, // 0x00000002
	};
	public enum class BreakPointOptions
	{
		Enabled=DEBUG_BREAKPOINT_ENABLED ,
		ThisClientOnly=DEBUG_BREAKPOINT_ADDER_ONLY ,
		GoOnly=DEBUG_BREAKPOINT_GO_ONLY ,
		OneShot=DEBUG_BREAKPOINT_ONE_SHOT ,
		Deferred=DEBUG_BREAKPOINT_DEFERRED 
	};

	public enum class ExceptionType
	{
		AccessViolation=EXCEPTION_ACCESS_VIOLATION,
		ArrayBoundsExceeded=EXCEPTION_ARRAY_BOUNDS_EXCEEDED,
		Breakpoint=EXCEPTION_BREAKPOINT,
		DatatypeMisalignment=EXCEPTION_DATATYPE_MISALIGNMENT,
		FloatDenormalOperand=EXCEPTION_FLT_DENORMAL_OPERAND,
		FloatDivideByZero=EXCEPTION_FLT_DIVIDE_BY_ZERO,
		FloatInexactResult=EXCEPTION_FLT_INEXACT_RESULT,
		FloatInvalidOperation=EXCEPTION_FLT_INVALID_OPERATION,
		FloatOverflow=EXCEPTION_FLT_OVERFLOW,
		FloatStackCheck=EXCEPTION_FLT_STACK_CHECK,
		FloatUnderflow=EXCEPTION_FLT_UNDERFLOW,
		PageGuardAccess=EXCEPTION_GUARD_PAGE,
		InvalidInstruction=EXCEPTION_ILLEGAL_INSTRUCTION,
		PageError=EXCEPTION_IN_PAGE_ERROR,
		IntegerDivideByZero=EXCEPTION_INT_DIVIDE_BY_ZERO,
		IntegerOverflow=EXCEPTION_INT_OVERFLOW,
		InvalidDisposition=EXCEPTION_INVALID_DISPOSITION,
		InvalidHandle=EXCEPTION_INVALID_HANDLE,
		NoncontinuableException=EXCEPTION_NONCONTINUABLE_EXCEPTION,
		PrivilegedInstruction=EXCEPTION_PRIV_INSTRUCTION,
		SingleStep=EXCEPTION_SINGLE_STEP,
		StackOverflow=EXCEPTION_STACK_OVERFLOW,
		//FrameConsolidationExecuted=STATUS_UNWIND_CONSOLIDATE,
		DException= (3 << 30) | (1 << 29) | (0 << 28) | ('D' << 16) | 1,
	};

	public enum class SessionStatus
	{
		Active=DEBUG_SESSION_ACTIVE,	//A debugger session has started.
		EndedBySessionTerminate=DEBUG_SESSION_END_SESSION_ACTIVE_TERMINATE,	//The session was ended by sending DEBUG_END_ACTIVE_TERMINATE to EndSession.
		EndedBySessionDetach=DEBUG_SESSION_END_SESSION_ACTIVE_DETACH,	//The session was ended by sending DEBUG_END_ACTIVE_DETACH to EndSession.
		EndedPassive=DEBUG_SESSION_END_SESSION_PASSIVE,	//The session was ended by sending DEBUG_END_PASSIVE to EndSession.
		Ended=DEBUG_SESSION_END,	//The target ran to completion, ending the session.
		TargetReboot=DEBUG_SESSION_REBOOT,	//The target computer rebooted, ending the session.
		TargetHibernate=DEBUG_SESSION_HIBERNATE,	//The target computer went into hibernation, ending the session.
		EngineFailure=DEBUG_SESSION_FAILURE,	//The engine was unable to continue the session.
	};

	public enum class WaitResult
	{
		OK=S_OK,
		TimeOut=S_FALSE,
		InterruptRequested=E_PENDING,
		Unexpected=E_UNEXPECTED,
		AlreadyWaiting=E_FAIL
	};
	
	public struct DArray
	{
		DWORD Length;
		DWORD Ptr;
	};
	
	ref class DebugSymbols;
	
	public ref struct DClassInfo
		{
		internal:
			DebugSymbols^ ds;
			DClassInfo(DebugSymbols^ dbgsym):ds(dbgsym)	{}
		public:
			String^ Name;
			ULONG64 Base;
			property DClassInfo^ BaseClass
			{
			public:
				DClassInfo^ get();
			}
		};

	public ref class CodeException
	{
	internal:
		CodeException(){}

	public:
		DClassInfo^ TypeInfo;
		String^ Message;
		String^ SourceFile;
		ULONG SourceLine;
		
		bool IsFirstChance;
		ULONG Type;
		bool IsContinuable;
		ULONG64 Address;
	};

	public ref struct DebugSymbolData
	{
	public:
		DebugSymbolData(String^ SymbolName, ULONG64 SymOffset)
		{
			Name=SymbolName;
			Offset=SymOffset;
		}	
		String^ Name;
		ULONG64 Offset;
	};
	
	public enum class DebugSymbolFlags
	{
		IsExpanded=DEBUG_SYMBOL_EXPANDED,
		IsReadonly=DEBUG_SYMBOL_READ_ONLY,
		IsArray=DEBUG_SYMBOL_IS_ARRAY,
		IsFloat=DEBUG_SYMBOL_IS_FLOAT,
		IsArgument=DEBUG_SYMBOL_IS_ARGUMENT,
		IsLocal=DEBUG_SYMBOL_IS_LOCAL,
	};

	public ref struct DebugScopedSymbol
	{
	public:
		DebugScopedSymbol()
		{}
		ULONG Depth;
		ULONG ParentId;
		DebugSymbolFlags^ Flags;
		//DEBUG_SYMBOL_ENTRY* SymbolData;
		ULONG Id;
		String^ Name;
		String^ TypeName;
		String^ TextValue;
		ULONG64 Offset;
		ULONG Size;
	};
}