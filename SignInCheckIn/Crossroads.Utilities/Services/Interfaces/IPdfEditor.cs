﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossroads.Utilities.Services.Interfaces
{
    public interface IPdfEditor
    {
        string PopulatePdfMergeFields(byte[] pdfFile, Dictionary<string, string> mergeValues);
    }
}
