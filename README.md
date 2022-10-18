<p align="center">
    <img src="Assets/Images/logo.png" height="200" />
</p>

## Introduction
Phantomity it's a native bridge between Unity Engine and Phantom wallet through either universal links or deeplinks. You don't need to think about storing private keys, managing cryptography operations, and sending transactions to Solana RPC. You make a game, the Phantomity does blockchain magic.

## Deeplinks

Phantomity supports two types of links: universal links (recommended) and deeplinks. Let's deep dive into setup both of that in the SDK.<br/>
To get more on how to enable deep linking inside Unity please read the [article](https://docs.unity3d.com/2020.3/Documentation/Manual/enabling-deep-linking.html).

### Deeplink
To setup deeplink you need to create an instance of [PhantomBridge](Assets/Phantomity/Scripts/PhantomBridge.cs)

```csharp
var scheme = "unitydl";
var appUrl = "https://example.com";
var phantomBridge = new PhantomBridge(scheme, appUrl);
```

where `scheme` is a string the same as `android:scheme` value in `<intent-filter>` for Android or *Project Settings -> Info -> The URL Types* for iOS
and `appUrl` is a url used to fetch app metadata (i.e. title, icon).

Current instance will use redirect link `unitydl://<method>`.

### Universal link
> **Note**<br/>
> Before testing the application with the universal link make sure it associated with a website. Read more about association for [Android](https://developer.android.com/training/app-links/verify-android-applinks#web-assoc) and [iOS](https://developer.apple.com/documentation/Xcode/supporting-associated-domains?language=objc).

To setup universal link you need to instantiate [LinkConfig](Assets/Phantomity/Scripts/DTO/LinkConfig.cs)

```csharp
var linkConfig = new LinkConfig
{
    Scheme = "https",
    Domain = "www.example.com",
    PathPrefix = "phantom",
};
```

with fields:
* `Scheme` - required, same as `android:scheme` value in `<intent-filter>` for Android or *Project Settings -> Info -> The URL Types* for iOS;
* `Domain` - required, same as `android:host` value in `<intent-filter>` for Android;
* `PathPrefix` - not required, same as `android:pathPrefix` in `<intent-filter>` for Android. Part of link between domain name and callback method name (https://example.com/<PathpPefix>/connect).

```csharp
var appUrl = "https://example.com";
var phantomBridge = new PhantomBridge(linkConfig, appUrl);
```

Current instance will use redirect link `https://example.com/phantom/<method>`.

## Methods
Phantomity supports all [provider](https://docs.phantom.app/integrating/deeplinks-ios-and-android/provider-methods) and [other](https://docs.phantom.app/integrating/deeplinks-ios-and-android/other-methods) methods provided by Phantom wallet.<br/>
To start work with methods just create an instantiate of [PhantomBridge](Assets/Phantomity/Scripts/PhantomBridge.cs) [one](#deeplink) or [another](#universal-link) way.

### Connect
In order to start interacting with Phantom, an app must first establish a connection. This connection request will prompt the user for permission to share their public key, indicating that they are willing to interact further.

```csharp
var address = await phantomBridge.Connect();
```

Instead of connecting, you can use the AutoConnect field to set up an automatic connection on the call with any of the provider methods.

```csharp
phantomBridge.AutoConnect = true;
```

### SignMessage

Once it's connected to Phantom, an app can request that the user signs a given message. Applications are free to write their own messages which will be displayed to users from within Phantom's signature prompt. Message signatures do not involve network fees and are a convenient way for apps to verify ownership of an address.

```csharp
var signature = await phantomBridge.SignMessage("HelloWorld");
```

### SignTransaction

The easiest and most recommended way to send a transaction is via [SignAndSendTransaction](#signandsendtransaction). It is safer for users, and a simpler API for developers, for Phantom to submit the transaction immediately after signing it instead of relying on the application to do so.<br/>
However, it is also possible for an app to request just the signature from Phantom.

```csharp
byte[] tx = new TransactionBuilder()
	// Add some payload
	.Serialize();

var signature = await phantomBridge.SignTransaction(tx);
```

> **Note**<br/>
> `TransactionBuilder` is not part of Phantomity.

### SignAllTransactions

Once an app is connected, it is also possible to sign multiple transactions at once. Unlike SignAndSendTransaction, Phantom will not submit these transactions to the network.

```csharp
var signature = await phantomBridge.SignAllTransactions(txs);
```

### SignAndSendTransaction

Once an application is connected to Phantom, it can prompt the user for permission to send transactions on their behalf.<br/>

```csharp
byte[] tx = new TransactionBuilder()
	// Add some payload
	.Serialize();

var signature = await phantomBridge.SignAndSendTransaction(tx);
```

Return the first signature in the transaction, which can be used as its transaction id.

> **Note**<br/>
> `TransactionBuilder` is not part of Phantomity.

### Disconnect
After an initial [Connect](#connect) event has taken place, an app may disconnect from Phantom at anytime. Once disconnected, Phantom will reject all signature requests until another connection is established.

```csharp
await phantomBridge.Disconnect();
```

### Browse
Phantomity provide a convenient way for users to open web apps within Phantom.
The method can be used before a [Connect](#connect) event takes places.

```csharp
var link = "https://example.com";
phantomBridge.Browse(link);
```

## How to redefine redirect methods names?

To understand why you need to redefine redirect methods names let's deep dive into the flesh of deeplinks communication between the app and Phantom.

When the app calls some method ([Connect](#connect) as an example) Phantomity generates a link with a method name

``https://phantom.app/ul/v1/connect``

When Phantom handled a request it calls the app with a redirect link with the same method name

``app://connect``

If you need to redefine redirect methods names for any purposes you need to instantiate [LinkConfig](Assets/Phantomity/Scripts/DTO/LinkConfig.cs) with a field `RedefinedMethods`

```csharp
var linkConfig = new LinkConfig
{
    Scheme = "app",
    RedefinedMethods = new Dictionary<string, string>
    {
        {PhantomMethods.Connect, "onPhantomConnected"},
        {PhantomMethods.SignMessage, "onMessageSigned"}
    }
};
```

## Error handling

Phantomity can throw [PhantomException](Assets/Phantomity/Scripts/Utils/PhantomException.cs) with Phantom specific [error codes](https://docs.phantom.app/integrating/errors).

```csharp
try
{
    // method call
}
catch (PhantomException e)
{
    Debug.Log(e.Message);
}
catch (InvalidOperationException e)
{
    Debug.Log(e.Message);
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/sidorovkirill/Phantomity/blob/08759fd665a45c9e006043051b38e4fe711160d1/LICENSE) file for details