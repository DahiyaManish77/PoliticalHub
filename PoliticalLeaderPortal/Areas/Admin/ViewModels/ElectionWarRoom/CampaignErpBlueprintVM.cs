using System.Collections.Generic;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class CampaignErpBlueprintVM
    {
        public CampaignErpBlueprintVM()
        {
            Pillars = new List<CampaignErpPillarVM>();
            DeliveryPhases = new List<CampaignErpPhaseVM>();
            GovernanceRules = new List<string>();
        }

        public string Title { get; set; }
        public string Subtitle { get; set; }
        public IList<CampaignErpPillarVM> Pillars { get; set; }
        public IList<CampaignErpPhaseVM> DeliveryPhases { get; set; }
        public IList<string> GovernanceRules { get; set; }
    }

    public class CampaignErpPillarVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Icon { get; set; }
        public string ActionName { get; set; }
        public IList<string> Capabilities { get; set; }
    }

    public class CampaignErpPhaseVM
    {
        public string Phase { get; set; }
        public string Focus { get; set; }
        public string Outcome { get; set; }
    }

    public class CampaignErpModuleVM
    {
        public CampaignErpModuleVM()
        {
            Sections = new List<CampaignErpModuleSectionVM>();
            SecurityRules = new List<string>();
            IntegrationNotes = new List<string>();
        }

        public string Name { get; set; }
        public string Purpose { get; set; }
        public string Status { get; set; }
        public string OwnerRole { get; set; }
        public IList<CampaignErpModuleSectionVM> Sections { get; set; }
        public IList<string> SecurityRules { get; set; }
        public IList<string> IntegrationNotes { get; set; }
    }

    public class CampaignErpModuleSectionVM
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IList<string> Fields { get; set; }
    }
}
