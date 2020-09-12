using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalBusDriver:Singleton<GlobalBusDriver> {
    [Header("Materials")]
    public Material dif;
    public Material dif_spec;
    public Material dif_spec_add_env;
    public Material none_spec_add_env;
    public Material dif_a_decal_over;
    [Header("Library")]
    public LoadWorld loadWorld;
}

public enum RoadType:byte { NoSidewalk, WithSidewalk, TerrainOnly };