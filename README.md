# KeySDK-WH

(https://www.practicalvr.com/assets/images/key_blue.png "KEYSDK")

## Target Unity Version: 2017.4.0f1

KEYSDK should seamlessly upgrade to all current versions of unity. Please re-import the PracticaManaged DLL if you run into issues upgrading.

## Setup Guide

1. Register at https://developer.practicalvr.com
2. Join discord.practicalvr.com for approval.
3. Create an application in the portal and copy your API Key.
4. *Important* - before importing, in player settings, set the "Scripting Runtime Version" to 4.6 and the "Scripting Backend" to .NET.
5. Import KEYSDK and StreamingAssets folders.
6. Place the "PracticalManager" and optional "PracticalCamera" prefabs into scene.
7. Insert API Key into Practical Manager.
8. Hit play and start developing!

*Bonus -- give us some feedback!*

## Theme Changes

Through October, developers that sign-up and create their first application will gain permanent access to the Cyber, Nature, Frost, and Fire themes. To check them out, simply deploy an application utilizing the KEYSDK with an active API Key and click the buttons in the portal to change your theme.

## Practical Location Builder

You can customize spawn groups with the Practical Location Builder script located on the Practical Manager prefab. Calls to the spawning API can only be made from async methods.

### For Example:

First, include the spawning system into your script:

```C#
using PracticalManaged.Practical.SpatialSpawning;
```

then create an async method:

```C#
private async void BlueSpawn()
```

call the "GetSpawnPoint" API to return a position:

```C#
SpawnLocation returnedLocation = await PracticalLocator.Instance.GetSpawnPoint("Yellow Group");
```

You can then access the returned location and rotation:

```C#
Vector3 position = returnedLocation.Position;
Quarternion rotation = returnedLocation.Rotation;
```

All together, it should look like this:

```C#
    private async void RedSpawn()
    {
        Debug.Log("Running Query");
        SpawnLocation returnedLocation = await PracticalLocator.Instance.GetSpawnPoint("Red Group");
        if (returnedLocation != null)
        {
            Debug.Log("Position: " + returnedLocation.Position);
            Instantiate(RedDisk, returnedLocation.Position, returnedLocation.Rotation);
            Debug.Log("Red disk placed");
        }
    }
```

There is an example folder that shows usage and has a scene to show functionality on device.

## Known Issues

* There is a slight hang at the start of the application as it downloads a new theme. UX improvement to come.
* The application will not start at all unless a valid API Key is provided. Check phantom spacing at the front and back of your key.
* Returned locations are not always as random or spaced out as we would like.
* The Practical Location Builder script throws a non-breaking error after pressing play.
* Microphone is not enabled after receiving the permission pop-up on Windows RS4 and up.

