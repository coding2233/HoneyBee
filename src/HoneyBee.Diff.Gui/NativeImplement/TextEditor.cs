using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{

    public unsafe class TextEditor : IDisposable
    {
        private IntPtr _igTextEditor;

        private string nativeText
        {
            get
            {
                byte* igTextPtr = igGetTextEditor(_igTextEditor);
                string igText = Util.StringFromPtr(igTextPtr);
                return igText;
            }
        }

        private string _text;
        public string text
        {
            get 
            {
                return _text; 
            }
            set
            {

                byte* native_label;
                int label_byteCount = 0;
                if (!string.IsNullOrEmpty(value))
                {
                    label_byteCount = Encoding.UTF8.GetByteCount(value);
                    if (label_byteCount > Util.StackAllocationSizeLimit)
                    {
                        native_label = Util.Allocate(label_byteCount + 1);
                    }
                    else
                    {
                        byte* native_label_stackBytes = stackalloc byte[label_byteCount + 1];
                        native_label = native_label_stackBytes;
                    }
                    int native_label_offset = Util.GetUtf8(value, native_label, label_byteCount);
                    native_label[native_label_offset] = 0;
                    igSetTextEditor(_igTextEditor, native_label);
                }
                else { native_label = null; }
                Util.Free(native_label);
                _text = nativeText;
            }
        }

        public Vector2 CursorPosition
        {
            get
            {
                Coordinates* igPos = igGetCursorPositionTextEditor(_igTextEditor);
                Vector2 pos = new Vector2(igPos->mLine, igPos->mColumn);
                return pos;
            }
        }
        public int TotalLines => igGetTotalLinesTextEditor(_igTextEditor);
        public bool IsOverwrite => igIsOverwriteTextEditor(_igTextEditor);
        public bool CanUndo => igCanUndoTextEditor(_igTextEditor);

        private bool _isTextChanged=false;
        private bool _lastTextChanged=false;
        public bool IsTextChanged
        {
            get
            {
                var textChanged = igIsTextChangedTextEditor(_igTextEditor);
                if (_lastTextChanged!=textChanged)
                {
                    _isTextChanged=false;
                    if (!string.IsNullOrEmpty(_text))
                    {
                        _isTextChanged=!_text.Equals(nativeText);
                    }
                    _lastTextChanged = textChanged;
                }
                return _isTextChanged;
            }
            set 
            {
                _isTextChanged = value;
            }
        } 

        private static uint[] _styleColors;

        public uint[] styleColors
        {
            get
            {
                return _styleColors;
            }
            set
            {
                igCustomPaletteTextEditor(_igTextEditor, value, value.Length);
            }
        }


        private int[] _flagLines;

        public int[] flagLines
        {
            get
            {
                return _flagLines;
            }
            set
            {
                _flagLines = value;
                if (_flagLines != null && _flagLines.Length > 0)
                {
                    igSetFlagLinesTextEditor(_igTextEditor, _flagLines, _flagLines.Length);
                }
            }
        }

        private bool _ignoreChildWindow;
        public bool ignoreChildWindow
        {
            get
            {
                return _ignoreChildWindow;
            }
            set
            {
                _ignoreChildWindow = value;
                igIgnoreChildTextEditor(_igTextEditor, _ignoreChildWindow);
            }
        }

        private bool _readOnly;
        public bool readOnly
        {
            get
            {
                return _readOnly;
            }
            set
            {
                _readOnly = value;
                igSetReadOnlyTextEditor(_igTextEditor, _readOnly);
            }
        }

        private static HashSet<TextEditor> _allTextEditor = new HashSet<TextEditor>();

 
        public static void SetStyle(uint[] colors)
        {
            foreach (var item in _allTextEditor)
            {
                item.styleColors = colors;
            }
            _styleColors = colors;
        }

        public TextEditor()
        {
            _igTextEditor = igNewTextEditor();
            //igSetPaletteTextEditor(_igTextEditor, _style);
            igCustomPaletteTextEditor(_igTextEditor, _styleColors,_styleColors.Length);
            readOnly = true;
            igSetShowWhitespacesTextEditor(_igTextEditor, false);
            _allTextEditor.Add(this);
        }

        public void Render(string title, Vector2 size, bool border = false)
        {
            if (_igTextEditor != IntPtr.Zero)
            {
                igRenderTextEditor(_igTextEditor, title, size, border);
            }
        }

        public void SetFlagPoints(int[] points,string iconText,string tipText)
        {
            var iconTextPointer = ToImguiCharPointer(iconText);
            //var tipTextPointer = ToImguiCharPointer(iconText);
            igSetFlagPointsTextEditor(_igTextEditor, points, points.Length, iconTextPointer, tipText);
            //Util.Free(iconTextPointer);
            //Util.Free(tipTextPointer);
        }

        public Vector4 GetFlagPointRect(int lineNo)
        {
            Vector4 rect;
            if (!igGetFlagPointRectTextEditor(_igTextEditor, lineNo,&rect))
            {
                rect = Vector4.Zero;
            }
            return rect;
        }

        public void Dispose()
        {
            _allTextEditor.Remove(this);
            if (_igTextEditor != IntPtr.Zero)
            {
                igDeleteTextEditor(_igTextEditor);
            }
        }


        private byte* ToImguiCharPointer(string value)
        {
            byte* native_label;
            int label_byteCount = 0;
            if (!string.IsNullOrEmpty(value))
            {
                label_byteCount = Encoding.UTF8.GetByteCount(value);
                if (label_byteCount > Util.StackAllocationSizeLimit)
                {
                    native_label = Util.Allocate(label_byteCount + 1);
                }
                else
                {
                    byte* native_label_stackBytes = stackalloc byte[label_byteCount + 1];
                    native_label = native_label_stackBytes;
                }
                int native_label_offset = Util.GetUtf8(value, native_label, label_byteCount);
                native_label[native_label_offset] = 0;
            }
            else { native_label = null; }
            return native_label;
        }

        public struct Coordinates
        {
          public int mLine, mColumn;
        }

        public struct ImVec4
        {
            public float x, y, z, w;
        }

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr igNewTextEditor();

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igDeleteTextEditor(IntPtr textEditor);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igRenderTextEditor(IntPtr textEditor, string title, Vector2 size, bool border = false);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igSetTextEditor(IntPtr textEditor, byte* text);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern byte* igGetTextEditor(IntPtr textEditor);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igSetPaletteTextEditor(IntPtr textEditor, int style);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igSetReadOnlyTextEditor(IntPtr textEditor, bool readOnly);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igSetShowWhitespacesTextEditor(IntPtr textEditor, bool show);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igSetFlagLinesTextEditor(IntPtr textEditor, int[] flagLines, int length);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igSetFlagPointsTextEditor(IntPtr textEditor, int[] points, int length, byte* flagPointText, string flagPointTipText);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool igGetFlagPointRectTextEditor(IntPtr textEditor, int lineNo,Vector4* rect);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igCustomPaletteTextEditor(IntPtr textEditor, uint[] colors, int length);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igIgnoreChildTextEditor(IntPtr textEditor, bool ignoreChild);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern Coordinates* igGetCursorPositionTextEditor(IntPtr textEditor);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern int igGetTotalLinesTextEditor(IntPtr textEditor);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool igIsOverwriteTextEditor(IntPtr textEditor);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool igCanUndoTextEditor(IntPtr textEditor);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool igIsTextChangedTextEditor(IntPtr textEditor);


    }
}
