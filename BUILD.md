# Build Configuration

## Supported Platforms

### iOS (Primary Target)
- **Bundle ID**: com.towerfusion.towerfusion4
- **Minimum iOS Version**: 11.0
- **Architecture**: ARM64
- **Orientation**: Landscape (optimized for gameplay)

### Android (Secondary Target)
- **Package Name**: com.towerfusion.towerfusion4
- **Minimum API Level**: 22 (Android 5.1)
- **Target API Level**: Latest available
- **Architecture**: ARM64, ARMv7

## Build Settings

### Quality Settings
- **iOS Default**: Low (optimized for mobile performance)
- **Android Default**: Low (optimized for mobile performance)
- **Editor/Standalone**: Medium

### Graphics Settings
- **Rendering Path**: Forward
- **Color Space**: Gamma (better mobile compatibility)
- **Graphics API**: 
  - iOS: Metal (automatic)
  - Android: OpenGL ES 3.0/Vulkan

### Audio Settings
- **Spatial Audio**: Disabled (2D game)
- **Audio Compression**: Vorbis for music, ADPCM for SFX

## Performance Optimizations

### Mobile Specific
- Texture compression enabled for target platforms
- Mipmaps disabled for UI elements
- Dynamic batching enabled for small meshes
- GPU Skinning disabled (2D sprites only)
- Multithreaded Rendering enabled where supported

### Memory Management
- Use ScriptableObjects for data to reduce memory duplication
- Object pooling recommended for frequently spawned objects
- Garbage collection optimization through proper resource management

## Deployment Instructions

### Using Unity Editor
1. Open Build Settings (Ctrl/Cmd + Shift + B)
2. Select target platform (iOS/Android)
3. Ensure MainScene is added to build
4. Click "Build" or "Build and Run"

### Using Build Scripts
- **Menu**: Tower Fusion > Build iOS
- **Menu**: Tower Fusion > Build Android

### Post-Build Steps

#### iOS
1. Open generated Xcode project
2. Configure signing certificates
3. Set deployment target to device/simulator
4. Archive and upload to App Store Connect

#### Android
1. Sign APK with keystore
2. Test on target devices
3. Upload to Google Play Console

## Testing Checklist

### Functionality
- [ ] Game starts without errors
- [ ] Wave progression works correctly
- [ ] Tower placement and upgrades function
- [ ] Enemy pathfinding works on all maps
- [ ] UI responds properly to touch input
- [ ] Game over/victory states trigger correctly

### Performance (On Device)
- [ ] Maintains stable framerate (30+ FPS)
- [ ] Memory usage stays under 500MB
- [ ] No visible stuttering during gameplay
- [ ] Fast app launch time (<3 seconds)

### Platform Specific
- [ ] Proper safe area handling (iPhone X+ notch)
- [ ] Correct orientation locking
- [ ] Home button/gesture handling
- [ ] Background/foreground app lifecycle

## Known Limitations

1. **Graphics**: Basic 2D sprites only (no complex effects)
2. **Audio**: No spatial audio support
3. **Networking**: Single-player only
4. **Platform Features**: No platform-specific integrations (GameCenter, Google Play Games)
5. **Accessibility**: Basic support only

## Future Enhancements

### Graphics
- Add particle effects for tower attacks
- Implement animated sprites for enemies
- Add visual feedback for damage numbers

### Gameplay
- Implement more tower types and upgrades  
- Add boss enemies with special mechanics
- Create achievement system

### Platform Integration
- GameCenter/Google Play leaderboards
- Cloud save support
- In-app purchases for premium content