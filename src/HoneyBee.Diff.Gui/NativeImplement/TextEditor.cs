using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{

    public class TextEditor:IDisposable
    {
        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr igNewTextEditor();

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igDeleteTextEditor(IntPtr textEditor);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igRenderTextEditor(IntPtr textEditor,string title,Vector2 size,bool border=false);
        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        private static extern void igSetTextEditor(IntPtr textEditor, string text);

        private IntPtr _igTextEditor;

        private string _text;
        public string text 
        { 
            get { return _text;} 
            set 
            { 
                _text = value;
                igSetTextEditor(_igTextEditor, _text);
            } 
        }

        public TextEditor()
        {
            _igTextEditor = igNewTextEditor();
        }

        public void Render(string title, Vector2 size, bool border = false)
        {
            if(_igTextEditor!=IntPtr.Zero)
                igRenderTextEditor(_igTextEditor, title, size, border);
        }

        public void Dispose()
        {
            if (_igTextEditor != IntPtr.Zero)
            {
                igDeleteTextEditor(_igTextEditor);
            }
        }

    }
}
