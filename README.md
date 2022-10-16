<p align="center">
    <img src="Assets/Images/logo.png" height="200" />
</p>

## Introduction
Phantomity it's a native bridge between Unity Engine and Phantom wallet through either universal links or deeplinks. You don't need to think about storing private keys, managing cryptography operations, and sending transactions to Solana RPC. You make a game, the Phantomity does blockchain magic.

## Deeplinks

Phantomity supports two types of links: universal links (recommended) and deeplinks. Let's deep dive into setup both of that in the SDK.<br/>
To get more on how to enable deep linking inside Unity please read the [article](https://docs.unity3d.com/2020.3/Documentation/Manual/enabling-deep-linking.html).

### Deeplink
To setup deeplink you need to create an instance of [PhantomityBridge](https://github.com/sidorovkirill/Phantomity/blob/08759fd665a45c9e006043051b38e4fe711160d1/Assets/Phantomity/Scripts/PhantomityBridge.cs)

```csharp
var scheme = "unitydl"
var appUrl = "https://example.com"
var phantomBridge = new PhantomityBridge(scheme, appUrl);
```

where `scheme` is a string the same as `android:scheme` value in `<intent-filter>` for Android or *Project Settings -> Info -> The URL Types* for iOS
and `appUrl` is a url used to fetch app metadata (i.e. title, icon).
### Universal link
 > **Before testing the application with the universal link make sure it associated with a website. Read more about association for [Android](https://developer.android.com/training/app-links/verify-android-applinks#web-assoc) and [iOS](https://developer.apple.com/documentation/Xcode/supporting-associated-domains?language=objc).**

To setup universal link you need to instantiate [LinkConfig](https://github.com/sidorovkirill/Phantomity/blob/08759fd665a45c9e006043051b38e4fe711160d1/Assets/Phantomity/Scripts/DTO/LinkConfig.cs)

```csharp
var linkConfig = new LinkConfig
{
    Scheme = "https",
    Domain = "www.ankr.com",
    PathPrefix = "phantom",
};
```

with fields:
* `Scheme` - required, same as `android:scheme` value in `<intent-filter>` for Android or *Project Settings -> Info -> The URL Types* for iOS;
* `Domain` - required, same as `android:host` value in `<intent-filter>` for Android;
* `PathPrefix` - not required, part of link between domain name and callback method name (https://example.com/path/prefix/onPhantomConnect).

```csharp
var appUrl = "https://example.com"
var phantomBridge = new PhantomityBridge(linkConfig, appUrl);
```

## Methods

## How to redefine methods names?

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/sidorovkirill/Phantomity/blob/08759fd665a45c9e006043051b38e4fe711160d1/LICENSE) file for details