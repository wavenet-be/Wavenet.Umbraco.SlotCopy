# Wavenet.Umbraco7.SlotCopy
Simple helper to copy user content from one Azure Slot to another.

![wavenet-be MyGet Build Status](https://www.myget.org/BuildSource/Badge/wavenet-be?identifier=67b426f1-4646-47a9-8e21-a3e4c97433e3)

## How to install
### MachineKey
To work, this package requires you to have a custom machine key in your web config or to specify the `UmbracoSlotCopy::ValidationKey`.  
The key will be use to secure exchanges between your slots.   

### Azure Settings
It's suggested that all settings are put on [Azure Configure app settings](https://docs.microsoft.com/en-us/azure/app-service/configure-common#configure-app-settings) as Slot Settings.

| Key                                 | Value                                                             | Comment                                             |
| ----------------------------------- | ----------------------------------------------------------------- | --------------------------------------------------- |
| UmbracoSlotCopy::ServerToSync       | https://your-production-website/umbraco/wavenet/slotcopy/getfiles | **Required** on Target Slot                         |
| UmbracoSlotCopy::PathsToSync        | \~/media,\~/css,\~/App_Data/UmbracoForms                          | _Optional_ on Source Slot                           |
| UmbracoSlotCopy::FilesToSyncPattern | \*.\*                                                             | _Optional_ on Source Slot                           |
| UmbracoSlotCopy::ValidationKey      | Your validation key                                               | Best to use only if you have no machinekey defined. |

### Synchronisation
When everything is set up, you simply make a request on your staging slot: https://your-staging-website/umbraco/wavenet/slotcopy/sync
and it will show the synchronisation progress.  
If you run the same on production, it will reject the request with a 404 (check based on `UmbracoSlotCopy::ServerToSync`)

### Integration / Build Server
Just add a step which makes the HTTP request.

## Packages
| **Stable Release**
|-
| [![NuGet](https://img.shields.io/nuget/v/Wavenet.Umbraco7.SlotCopy.svg)](https://www.nuget.org/packages/Wavenet.Umbraco7.SlotCopy)
| **Early Access**
| [![MyGet](https://img.shields.io/myget/wavenet-be/vpre/Wavenet.Umbraco7.SlotCopy.svg)](https://www.myget.org/feed/wavenet-be/package/nuget/Wavenet.Umbraco7.SlotCopy)
