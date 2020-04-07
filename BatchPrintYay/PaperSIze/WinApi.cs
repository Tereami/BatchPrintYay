using System;
using System.Runtime.InteropServices;
using System.Security;


namespace BatchPrintYay
{
    public static class WinApi
    {
        #region Constants

        public const int PRINTER_ACCESS_USE = 0x00000008; // позволить выполнять базовые  задачи
        public const int PRINTER_ACCESS_ADMINISTER = 0x00000004; // позволить выполнять задачи на уровне администратора, такие как SetPrinter


        #endregion Constants

        #region Structures

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct PrinterDefaults
        {
            [MarshalAs(UnmanagedType.LPTStr)] public String pDatatype;
            public IntPtr pDevMode;
            [MarshalAs(UnmanagedType.I4)] public int DesiredAccess;
        };
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]

        internal struct Size
        {
            public Int32 width;
            public Int32 height;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct Rect
        {
            public Int32 left;
            public Int32 top;
            public Int32 right;
            public Int32 bottom;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct FormInfo1
        {
            public UInt32 Flags;
            public String pName;
            public Size Size;
            public Rect ImageableArea;
        };

        #endregion Structures

        #region Functions
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinter", SetLastError = true,
                     CharSet = CharSet.Unicode, ExactSpelling = false, CallingConvention = CallingConvention.StdCall),
                SuppressUnmanagedCodeSecurityAttribute()]
        internal static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPTStr)]
            string printerName,
                    out IntPtr phPrinter,
                    ref PrinterDefaults pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true,
             CharSet = CharSet.Unicode, ExactSpelling = false,
             CallingConvention = CallingConvention.StdCall), SuppressUnmanagedCodeSecurityAttribute()]
        internal static extern bool ClosePrinter(IntPtr phPrinter);


        [DllImport("winspool.Drv", EntryPoint = "AddFormW", SetLastError = true,
          CharSet = CharSet.Unicode, ExactSpelling = true,
          CallingConvention = CallingConvention.StdCall), SuppressUnmanagedCodeSecurityAttribute()]
        internal static extern bool AddForm(
      IntPtr phPrinter, [MarshalAs(UnmanagedType.I4)] int level, ref FormInfo1 form);


        [DllImport("winspool.Drv", EntryPoint = "DeleteForm", SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall),
       SuppressUnmanagedCodeSecurityAttribute()]
        internal static extern bool DeleteForm(IntPtr phPrinter, [MarshalAs(UnmanagedType.LPTStr)] string pName);


        [DllImport("kernel32.dll", EntryPoint = "GetLastError", SetLastError = false,
             ExactSpelling = true, CallingConvention = CallingConvention.StdCall), SuppressUnmanagedCodeSecurityAttribute()]
        internal static extern Int32 GetLastError();

        #endregion Functions
    }
}

