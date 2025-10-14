import uuid

projectiles = [
    ("FireProjectile", "ff5014ff"),
    ("WaterProjectile", "3c96ffff"),
    ("EarthProjectile", "8b5a2bff"),
    ("AirProjectile", "c8e6ffff"),
    ("LightningProjectile", "ffff64ff"),
    ("IceProjectile", "96dcffff"),
    ("PoisonProjectile", "64c832ff"),
    ("DarkProjectile", "783296ff"),
    ("HolyProjectile", "fff096ff"),
    ("MagicProjectile", "c864ffff"),
    ("ExplosiveProjectile", "ff9600ff"),
    ("SniperProjectile", "dcdcdcff"),
]

# We need to reference the sprite GUIDs from the meta files we created
sprite_refs = {
    "FireProjectile": "SPRITE_GUID_FIRE",
    "WaterProjectile": "SPRITE_GUID_WATER",
    "EarthProjectile": "SPRITE_GUID_EARTH",
    "AirProjectile": "SPRITE_GUID_AIR",
    "LightningProjectile": "SPRITE_GUID_LIGHTNING",
    "IceProjectile": "SPRITE_GUID_ICE",
    "PoisonProjectile": "SPRITE_GUID_POISON",
    "DarkProjectile": "SPRITE_GUID_DARK",
    "HolyProjectile": "SPRITE_GUID_HOLY",
    "MagicProjectile": "SPRITE_GUID_MAGIC",
    "ExplosiveProjectile": "SPRITE_GUID_EXPLOSIVE",
    "SniperProjectile": "SPRITE_GUID_SNIPER",
}

# Read sprite GUIDs from meta files
import os
sprite_dir = "../../Sprites/Projectiles"
for name in sprite_refs.keys():
    meta_file = os.path.join(sprite_dir, f"{name}.png.meta")
    if os.path.exists(meta_file):
        with open(meta_file, 'r') as f:
            for line in f:
                if line.startswith('guid:'):
                    guid = line.split(':')[1].strip()
                    sprite_refs[name] = guid
                    break

prefab_template = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &{root_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {transform_id}}}
  - component: {{fileID: {sprite_id}}}
  - component: {{fileID: {rigidbody_id}}}
  - component: {{fileID: {collider_id}}}
  - component: {{fileID: {script_id}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Projectile
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{transform_id}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {root_id}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!212 &{sprite_id}
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {root_id}}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 15
  m_Sprite: {{fileID: 21300000, guid: {sprite_guid}, type: 3}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {{x: 0.64, y: 0.64}}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
--- !u!50 &{rigidbody_id}
Rigidbody2D:
  serializedVersion: 4
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {root_id}}}
  m_BodyType: 1
  m_Simulated: 1
  m_UseFullKinematicContacts: 0
  m_UseAutoMass: 0
  m_Mass: 1
  m_LinearDrag: 0
  m_AngularDrag: 0.05
  m_GravityScale: 0
  m_Material: {{fileID: 0}}
  m_Interpolate: 0
  m_SleepingMode: 1
  m_CollisionDetection: 1
  m_Constraints: 0
--- !u!58 &{collider_id}
CircleCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {root_id}}}
  m_Enabled: 1
  m_Density: 1
  m_Material: {{fileID: 0}}
  m_IsTrigger: 1
  m_UsedByEffector: 0
  m_UsedByComposite: 0
  m_Offset: {{x: 0, y: 0}}
  serializedVersion: 2
  m_Radius: 0.15
--- !u!114 &{script_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {root_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: YOUR_PROJECTILE_SCRIPT_GUID, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  spriteRenderer: {{fileID: {sprite_id}}}
  rb2D: {{fileID: {rigidbody_id}}}
  projectileCollider: {{fileID: {collider_id}}}
  impactEffectPrefab: {{fileID: 0}}
  trailEffectPrefab: {{fileID: 0}}
"""

for name, color in projectiles:
    # Generate unique IDs for each component
    root_id = str(abs(hash(name + "root")) % 9000000000 + 1000000000)
    transform_id = str(abs(hash(name + "transform")) % 9000000000 + 1000000000)
    sprite_id = str(abs(hash(name + "sprite")) % 9000000000 + 1000000000)
    rigidbody_id = str(abs(hash(name + "rb")) % 9000000000 + 1000000000)
    collider_id = str(abs(hash(name + "collider")) % 9000000000 + 1000000000)
    script_id = str(abs(hash(name + "script")) % 9000000000 + 1000000000)
    
    sprite_guid = sprite_refs.get(name, "00000000000000000000000000000000")
    
    prefab_content = prefab_template.format(
        name=name,
        root_id=root_id,
        transform_id=transform_id,
        sprite_id=sprite_id,
        rigidbody_id=rigidbody_id,
        collider_id=collider_id,
        script_id=script_id,
        sprite_guid=sprite_guid,
        color=color
    )
    
    prefab_filename = f"{name}.prefab"
    with open(prefab_filename, 'w') as f:
        f.write(prefab_content)
    print(f"Created {prefab_filename}")
    
    # Create meta file for prefab
    prefab_guid = uuid.uuid4().hex
    meta_content = f"""fileFormatVersion: 2
guid: {prefab_guid}
PrefabImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    meta_filename = f"{prefab_filename}.meta"
    with open(meta_filename, 'w') as f:
        f.write(meta_content)

print("\nAll projectile prefabs created successfully!")
print("\nNOTE: You'll need to update the Projectile script GUID in Unity.")
print("Open any prefab and assign the Projectile component properly.")
