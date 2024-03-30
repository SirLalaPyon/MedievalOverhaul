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
        public List<string> settings;
        public bool inverse = false;
        private readonly List<PatchOperation> operations = new List<PatchOperation>();
        private PatchOperation lastFailedOperation;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (HasSetting() && !inverse)
            {
                return ApplyPatches(xml);
            }
            if (!HasSetting() && inverse)
            {
                return ApplyPatches(xml);
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
        public bool HasSetting()
        {
            for (int i = 0; i < settings.Count(); i++)
            {
                if (MedievalOverhaulSettings.settings.toggleSettings.Contains(settings[i]))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
