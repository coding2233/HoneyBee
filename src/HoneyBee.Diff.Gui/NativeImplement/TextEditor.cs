using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{

    public class TextEditor : IDisposable
    {
        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr igNewTextEditor();

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igDeleteTextEditor(IntPtr textEditor);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igRenderTextEditor(IntPtr textEditor, string title, Vector2 size, bool border = false);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igSetTextEditor(IntPtr textEditor, string text);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igSetPaletteTextEditor(IntPtr textEditor, int style);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igSetReadOnlyTextEditor(IntPtr textEditor, bool readOnly);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igSetShowWhitespacesTextEditor(IntPtr textEditor, bool show);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igSetFlagLinesTextEditor(IntPtr textEditor, int[] flagLines, int length);

        private IntPtr _igTextEditor;

        private string _text;
        public string text
        {
            get { return _text; }
            set
            {
                _text = value;
                igSetTextEditor(_igTextEditor, _text);
            }
        }

        private static int _style;
        public int style
        {
            get
            {
                return _style;
            }
            set
            {
                igSetPaletteTextEditor(_igTextEditor, value);
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

        private static HashSet<TextEditor> _allTextEditor = new HashSet<TextEditor>();

        public static void SetStyle(int style)
        {
            if (style > 1)
                style = 1;
            foreach (var item in _allTextEditor)
            {
                item.style = style;
            }
            _style = style;
        }

        public TextEditor()
        {
            _igTextEditor = igNewTextEditor();
            igSetPaletteTextEditor(_igTextEditor, _style);
            igSetReadOnlyTextEditor(_igTextEditor, true);
            igSetReadOnlyTextEditor(_igTextEditor, true);
            igSetShowWhitespacesTextEditor(_igTextEditor, false);
            _allTextEditor.Add(this);
        }

        public void Render(string title, Vector2 size, bool border = false)
        {
            if (_igTextEditor != IntPtr.Zero)
                igRenderTextEditor(_igTextEditor, title, size, border);
        }


        public void Dispose()
        {
            _allTextEditor.Remove(this);
            if (_igTextEditor != IntPtr.Zero)
            {
                igDeleteTextEditor(_igTextEditor);
            }
        }

    }
}
