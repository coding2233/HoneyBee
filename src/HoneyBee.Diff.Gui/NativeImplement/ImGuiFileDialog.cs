using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    [System.Flags]
    public enum ImGuiFileDialogFlags:int
    {
        ImGuiFileDialogFlags_None = 0,
        ImGuiFileDialogFlags_ConfirmOverwrite = (1 << 0),                           // show confirm to overwrite dialog
        ImGuiFileDialogFlags_DontShowHiddenFiles = (1 << 1),                        // dont show hidden file (file starting with a .)
        ImGuiFileDialogFlags_DisableCreateDirectoryButton = (1 << 2),               // disable the create directory button
        ImGuiFileDialogFlags_HideColumnType = (1 << 3),                             // hide column file type
        ImGuiFileDialogFlags_HideColumnSize = (1 << 4),                             // hide column file size
        ImGuiFileDialogFlags_HideColumnDate = (1 << 5),                             // hide column file date
//# ifdef USE_THUMBNAILS
//        ImGuiFileDialogFlags_DisableThumbnailMode = (1 << 6),                       // disable the thumbnail mode
//#endif
        ImGuiFileDialogFlags_Default = ImGuiFileDialogFlags_ConfirmOverwrite
    };

    public class ImGuiFileDialog
    {

       

        private static IntPtr _dialogContext;
        private static IntPtr dialogContext
        {
            get
            {
                if (_dialogContext == IntPtr.Zero)
                {
                    _dialogContext = IGFD_Create();
                }
                return _dialogContext;
            }
        }

        private const string _dialogKey = "Global File Dialog";
        private static string _displayKey = string.Empty;
        private static Action<string> _selectFolderCallBack;
        private static Action<string> _selectFilePathCallback;

        public static void OpenDialog(string key, string title, string filter, string path, string fileName = "", int countSelectionMax = 1, object userData = null, ImGuiFileDialogFlags flag = ImGuiFileDialogFlags.ImGuiFileDialogFlags_None)
        {
            IGFD_OpenDialog(dialogContext, key, title, filter, path, fileName, countSelectionMax, userData, (int)flag);
        }

        public static void OpenModal(string key, string title, string filter, string path, string fileName = "", int countSelectionMax = 1, object userData = null, ImGuiFileDialogFlags flag = ImGuiFileDialogFlags.ImGuiFileDialogFlags_None)
        {
            IGFD_OpenModal(dialogContext, key, title, filter, path, fileName, countSelectionMax, userData, (int)flag);
        }

        public static void OpenFolder(Action<string> selectCallback,string path="./")
        {
            _selectFolderCallBack = selectCallback;
            OpenFile(selectCallback, path,null);
        }

        public static void OpenFile(Action<string> selectCallback,string path="./",string filter= ".*")//".cs,.txt,.c,.cpp,.h,.meta,.prefab"
        {
            _displayKey = _dialogKey;
            _selectFilePathCallback = selectCallback;
            OpenModal(_displayKey, _displayKey, filter, path);
        }

        public static bool Display(string key,ImGuiWindowFlags flag,Vector2 minSize,Vector2 maxSize)
        {
            bool result = IGFD_DisplayDialog(dialogContext, key, (int)flag, minSize, maxSize);
            return result;
        }

        public static void Display()
        {
            if (!string.IsNullOrEmpty(_displayKey))
            {
                Vector2 maxSize = ImGui.GetWindowSize();
                Vector2 minSize = maxSize * 0.3f;
                try
                {
                    if (Display(_displayKey, ImGuiWindowFlags.None, minSize, maxSize))
                    {
                        string selectPath = string.Empty;
                        if (ImGuiFileDialog.IsOK())
                        {
                            if (_selectFolderCallBack != null)
                            {
                                selectPath=IGFD_GetFilePathName(dialogContext);
                                _selectFolderCallBack.Invoke(selectPath);
                            }
                            else if (_selectFilePathCallback != null)
                            {
                                selectPath = IGFD_GetCurrentPath(dialogContext);
                                selectPath = Path.Combine(selectPath, IGFD_SelectionFilePath(dialogContext));
                                _selectFilePathCallback.Invoke(selectPath);
                            }
                            Console.WriteLine(selectPath);
                        }
                        _selectFilePathCallback = null;
                        _selectFolderCallBack = null;
                        _displayKey = string.Empty;
                        ImGuiFileDialog.Close();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unconventional path: {e.ToString()}");
                }
            }
        }

        public static void Close()
        {
            IGFD_CloseDialog(dialogContext);
        }

        public static bool IsOK()
        {
            return IGFD_IsOk(dialogContext);
        }

        public static string GetFilePathName()
        {
            return IGFD_GetFilePathName(dialogContext);
        }

        public static string GetCurrentPath()
        {
            return IGFD_GetCurrentFileName(dialogContext);
        }

        struct IGFD_Selection_Pair
        {
            public string fileName;
            public string filePathName;
        }

        struct IGFD_Selection
        {
            public IGFD_Selection_Pair[] table;
            public int count;
        }

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr IGFD_Create();

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void IGFD_Destroy(IntPtr vContext);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void IGFD_OpenDialog(IntPtr vContext, string vKey, string vTitle, string vFilters, string vPath, string vFileName, int vCountSelectionMax, object userData, int flags);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void IGFD_OpenModal(IntPtr vContext, string vKey, string vTitle, string vFilters, string vPath, string vFileName, int vCountSelectionMax, object userData, int flags);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool IGFD_DisplayDialog(IntPtr vContext, string vKey, int vFlags, Vector2 vMinSize, Vector2 vMaxSize);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void IGFD_CloseDialog(IntPtr vContext);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool IGFD_IsOk(IntPtr vContext);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern string IGFD_GetFilePathName(IntPtr vContext);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern string IGFD_GetCurrentFileName(IntPtr vContext);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern string IGFD_GetCurrentPath(IntPtr vContext);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern string IGFD_GetCurrentFilter(IntPtr vContext);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern string IGFD_SelectionFilePath(IntPtr vContext);

        

    }
}
