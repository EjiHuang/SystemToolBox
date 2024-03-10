using System;
using System.Collections.Generic;
using System.Management;

namespace SystemToolBox.ViewModel
{
    public class Win32Bios
    {
        public List<Win32Bios> Win32Bioses;

        public ushort[] BiosCharacteristics;
        public string[] BiosVersion;
        public string BuildNumber;
        public string Caption;
        public string CodeSet;
        public string CurrentLanguage;
        public string Description;
        public string IdentificationCode;
        public ushort? InstallableLanguages;
        public DateTime? InstallDate;
        public string LanguageEdition;
        public string[] ListOfLanguages;
        public string Manufacturer;
        public string Name;
        public string OtherTargetOs;
        public bool? PrimaryBios;
        public string ReleaseDate;
        public string SerialNumber;
        public string SmbiosbiosVersion;
        public ushort? SmbiosMajorVersion;
        public ushort? SmbiosMinorVersion;
        public bool? SmbiosPresent;
        public string SoftwareElementId;
        public ushort? SoftwareElementState;
        public string Status;
        public ushort? TargetOperatingSystem;
        public string Version;

        public Win32Bios()
        {
            
        }

        public void GetSystemInfo()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
            var collection = searcher.Get();

            Win32Bioses = new List<Win32Bios>();

            foreach (var o in collection)
            {
                var obj = (ManagementObject)o;
                Win32Bioses.Add(new Win32Bios
                {
                    BiosCharacteristics = (ushort[])obj["BiosCharacteristics"],
                    BiosVersion = (string[])obj["BIOSVersion"],
                    BuildNumber = (string)obj["BuildNumber"],
                    Caption = (string)obj["Caption"],
                    CodeSet = (string)obj["CodeSet"],
                    CurrentLanguage = (string)obj["CurrentLanguage"],
                    Description = (string)obj["Description"],
                    IdentificationCode = (string)obj["IdentificationCode"],
                    InstallableLanguages = (ushort?)obj["InstallableLanguages"],
                    InstallDate = (DateTime?)obj["InstallDate"],
                    LanguageEdition = (string)obj["LanguageEdition"],
                    ListOfLanguages = (string[])obj["ListOfLanguages"],
                    Manufacturer = (string)obj["Manufacturer"],
                    Name = (string)obj["Name"],
                    OtherTargetOs = (string)obj["OtherTargetOS"],
                    PrimaryBios = (bool?)obj["PrimaryBIOS"],
                    ReleaseDate = (string)obj["ReleaseDate"],
                    SerialNumber = (string)obj["SerialNumber"],
                    SmbiosbiosVersion = (string)obj["SMBIOSBIOSVersion"],
                    SmbiosMajorVersion = (ushort?)obj["SMBIOSMajorVersion"],
                    SmbiosMinorVersion = (ushort?)obj["SMBIOSMinorVersion"],
                    SmbiosPresent = (bool?)obj["SMBIOSPresent"],
                    SoftwareElementId = (string)obj["SoftwareElementID"],
                    SoftwareElementState = (ushort?)obj["SoftwareElementState"],
                    Status = (string)obj["Status"],
                    TargetOperatingSystem = (ushort?)obj["TargetOperatingSystem"],
                    Version = (string)obj["Version"]
                });
            }
        }
    }
}