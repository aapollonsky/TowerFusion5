import os

prefabs = [
    "FireProjectile.prefab",
    "WaterProjectile.prefab",
    "EarthProjectile.prefab",
    "AirProjectile.prefab",
    "IceProjectile.prefab",
    "PoisonProjectile.prefab",
    "DarkProjectile.prefab",
    "HolyProjectile.prefab",
    "MagicProjectile.prefab",
    "ExplosiveProjectile.prefab",
    "SniperProjectile.prefab"
]

correct_guid = "adb22c109a93484db8be6b3fea878069"

for prefab_name in prefabs:
    if os.path.exists(prefab_name):
        with open(prefab_name, 'r') as f:
            content = f.read()
        
        updated_content = content.replace('YOUR_PROJECTILE_SCRIPT_GUID', correct_guid)
        
        with open(prefab_name, 'w') as f:
            f.write(updated_content)
        
        print(f"✓ Updated {prefab_name}")
    else:
        print(f"✗ {prefab_name} not found")

print("\nAll prefabs updated with correct Projectile script GUID!")
