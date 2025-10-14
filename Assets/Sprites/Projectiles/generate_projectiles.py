from PIL import Image, ImageDraw
import math

def create_basic_projectile(filename, size, color, shape="circle"):
    """Create a basic projectile sprite"""
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    center = size // 2
    
    if shape == "circle":
        # Simple circle projectile
        radius = size // 3
        draw.ellipse([center - radius, center - radius, center + radius, center + radius], 
                     fill=color, outline=(255, 255, 255, 200))
    elif shape == "arrow":
        # Arrow-shaped projectile pointing right
        points = [
            (size * 0.2, center),
            (size * 0.7, center - size * 0.2),
            (size * 0.7, center - size * 0.1),
            (size * 0.9, center),
            (size * 0.7, center + size * 0.1),
            (size * 0.7, center + size * 0.2)
        ]
        draw.polygon(points, fill=color, outline=(255, 255, 255, 200))
    elif shape == "star":
        # Star-shaped projectile
        points = []
        for i in range(10):
            angle = (i * 36 - 90) * math.pi / 180
            r = (size * 0.35) if i % 2 == 0 else (size * 0.15)
            x = center + r * math.cos(angle)
            y = center + r * math.sin(angle)
            points.append((x, y))
        draw.polygon(points, fill=color, outline=(255, 255, 255, 150))
    elif shape == "diamond":
        # Diamond-shaped projectile
        points = [
            (center, size * 0.15),
            (size * 0.85, center),
            (center, size * 0.85),
            (size * 0.15, center)
        ]
        draw.polygon(points, fill=color, outline=(255, 255, 255, 200))
    elif shape == "bolt":
        # Lightning bolt style
        points = [
            (center + size * 0.1, size * 0.1),
            (center, center - size * 0.1),
            (center + size * 0.15, center),
            (center - size * 0.05, center),
            (center, center + size * 0.1),
            (center - size * 0.1, size * 0.9),
            (center - size * 0.05, center + size * 0.1),
            (center + size * 0.05, center + size * 0.1)
        ]
        draw.polygon(points, fill=color, outline=(255, 255, 255, 180))
    elif shape == "fire":
        # Flame-like shape
        points = []
        for i in range(8):
            angle = (i * 45 - 90) * math.pi / 180
            r = size * 0.3 + (size * 0.1 * (i % 2))
            x = center + r * math.cos(angle)
            y = center + r * math.sin(angle)
            points.append((x, y))
        draw.polygon(points, fill=color, outline=(255, 200, 100, 200))
    
    # Add a subtle glow effect
    glow_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow_img)
    glow_radius = size // 2
    for i in range(3):
        alpha = 30 - (i * 10)
        r = glow_radius - (i * (glow_radius // 6))
        glow_color = (*color[:3], alpha)
        glow_draw.ellipse([center - r, center - r, center + r, center + r], 
                         fill=glow_color)
    
    # Composite glow behind main sprite
    final_img = Image.alpha_composite(glow_img, img)
    final_img.save(filename)
    print(f"Created {filename}")

# Create various projectile types
projectiles = [
    ("FireProjectile.png", 64, (255, 80, 20, 255), "fire"),
    ("WaterProjectile.png", 64, (60, 150, 255, 255), "circle"),
    ("EarthProjectile.png", 64, (139, 90, 43, 255), "diamond"),
    ("AirProjectile.png", 64, (200, 230, 255, 255), "arrow"),
    ("LightningProjectile.png", 64, (255, 255, 100, 255), "bolt"),
    ("IceProjectile.png", 64, (150, 220, 255, 255), "diamond"),
    ("PoisonProjectile.png", 64, (100, 200, 50, 255), "circle"),
    ("DarkProjectile.png", 64, (120, 50, 150, 255), "star"),
    ("HolyProjectile.png", 64, (255, 240, 150, 255), "star"),
    ("MagicProjectile.png", 64, (200, 100, 255, 255), "circle"),
    ("ExplosiveProjectile.png", 64, (255, 150, 0, 255), "circle"),
    ("SniperProjectile.png", 64, (220, 220, 220, 255), "arrow"),
]

for filename, size, color, shape in projectiles:
    create_basic_projectile(filename, size, color, shape)

print("\nAll projectile sprites created successfully!")
