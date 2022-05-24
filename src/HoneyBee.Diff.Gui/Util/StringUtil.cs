using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public static class StringUtil
    {
        //public unsafe static byte* ToImGuiUtf8(this string label)
        //{
        //    byte* native_label;
        //    int label_byteCount = 0;
        //    if (label != null)
        //    {
        //        label_byteCount = Encoding.UTF8.GetByteCount(label);
        //        if (label_byteCount > Util.StackAllocationSizeLimit)
        //        {
        //            native_label = Util.Allocate(label_byteCount + 1);
        //        }
        //        else
        //        {
        //            byte* native_label_stackBytes = stackalloc byte[label_byteCount + 1];
        //            native_label = native_label_stackBytes;
        //        }
        //        int native_label_offset = Util.GetUtf8(label, native_label, label_byteCount);
        //        native_label[native_label_offset] = 0;
        //    }
        //    else { native_label = null; }
        //    return native_label;
        //}

        public static string ToSizeString(this long size)
        {
            if (size < 1024)
            {
                return $"{size} Byte";
            }
            else if (size < 1024 * 1024)
            {
                return $"{(size / 1024.0f).ToString("f2")} KB";
            }
            else if (size < 1024 * 1024 * 1024)
            {
                return $"{(size / 1024.0f / 1024.0f).ToString("f2")} MB";
            }
            else
            {
                return $"{(size / 1024.0f / 1024.0f / 1024.0f).ToString("f2")} GB";
            }
        }
    }
}
