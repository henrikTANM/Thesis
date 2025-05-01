using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StarValues : ScriptableObject
{   
    public float scale;

    public Color color;
    public Color cellColor;

    public float cellDensity;
    public float solarFlare;
    public float cellSpeed;

    public List<string> starNames = new()
        {
            "XR-1047", "ZQ-82", "HDX 2371", "TYR-55B", "KJ-442", "LPM-31", "VX-77", "BZ-421", "GTR-4", "ML-85",
            "RZ-309", "XE-19", "K8-ZX", "TR-901", "QU-13", "TYC-73A", "HDN-88", "AK-237", "GX-29", "JP-12",
            "YV-633", "LR-210", "MS-44", "KP-16", "WN-802", "TS-91", "BV-4X", "HF-611", "QE-10", "OZ-57",
            "JN-88", "RC-71", "DM-204", "XA-7", "VR-299", "CZ-33", "LP-98", "NK-12", "ST-114", "HY-6",
            "BG-422", "UL-73", "PQ-10", "R4-TX", "DZ-32", "VG-200", "XS-77", "WQ-81", "TRX-59", "LT-04",

            "Altheran", "Bequari", "Caelion", "Drazhar", "Elnara", "Lyquar", "Myzara", "Naelion", "Quellor", "Raxion",
            "Sothara", "Thulex", "Uvaran", "Vexir", "Walyra", "Xenthos", "Yrradis", "Zubarae", "Caphrion", "Deltari",
            "Elanthe", "Gaiath", "Jurellan", "Kelthas", "Nivaros", "Oltheris", "Quentari", "Seleron", "Umbrae", "Varnix",
            "Westari", "Yedra", "Altivar", "Belmara", "Corvalis", "Elarith", "Faejor", "Herion", "Iskareth", "Korvius",
            "Maethion", "Nelzar", "Opheliad", "Pyrion", "Rindomar", "Sypherion", "Taurellus", "Umbraxis", "Virellion", "Xystara"
        };
}
