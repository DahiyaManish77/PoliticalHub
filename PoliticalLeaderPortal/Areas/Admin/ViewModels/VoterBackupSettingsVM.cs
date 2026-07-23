using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class VoterBackupSettingsVM
    {
        public int BackupSettingId { get; set; }

        [Display(Name = "Enable Auto Backup")]
        public bool AutoBackupEnabled { get; set; }

        [Display(Name = "Keep Latest Files")]
        [Range(1, 500)]
        public int KeepLatestFiles { get; set; }

        [Display(Name = "Mirror To Google Drive Folder")]
        public bool MirrorToDriveFolder { get; set; }

        [Display(Name = "Google Drive Synced Folder Path")]
        [StringLength(500)]
        public string DriveMirrorFolderPath { get; set; }

        [Display(Name = "Last Backup File")]
        public string LastBackupFilePath { get; set; }

        [Display(Name = "Last Mirror File")]
        public string LastMirrorFilePath { get; set; }

        [Display(Name = "Last Backup Status")]
        public string LastBackupStatus { get; set; }
    }
}
