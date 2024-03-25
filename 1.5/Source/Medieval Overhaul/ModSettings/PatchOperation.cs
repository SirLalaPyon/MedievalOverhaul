using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using System.Xml;

namespace MedievalOverhaul
{
    public class PatchOperation_ToggleSettings : PatchOperation
    {
        public string settings;
        public bool inverse = false;
        private readonly List<PatchOperation> operations = new List<PatchOperation>();
        private PatchOperation lastFailedOperation;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (MedievalOverhaul_Settings.settingMode.ContainsKey(settings))
            {
                MedievalOverhaul_Settings.settingMode.TryGetValue(settings, out bool value);
                if (value && !inverse)
                {
                    return ApplyPatches(xml);
                }
                if (!value && inverse)
                {
                    return ApplyPatches(xml);
                }
            }
            return true;
        }
        private bool ApplyPatches(XmlDocument xml)
        {
            foreach (PatchOperation operation in this.operations)
            {
                if (!operation.Apply(xml))
                {
                    this.lastFailedOperation = operation;
                    return false;
                }
            }
            return true;
        }

    }
}
