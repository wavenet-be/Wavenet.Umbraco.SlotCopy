# Wavenet.Umbraco7.SlotCopy
Simple helper to copy user content from one Azure Slot to another.

![wavenet-be MyGet Build Status](https://www.myget.org/BuildSource/Badge/wavenet-be?identifier=67b426f1-4646-47a9-8e21-a3e4c97433e3)

## How to install
### MachineKey
To work, this package requires you to have a custom machine key in your web config.  
The key will be use to secure exchanges between your slots.   
If you do not have a machine key you can generate one on https://www.getakey.online/.

Sample:
```xml
<machineKey  
    validationKey="DE303CE4B5533B94485078C87086105B656121A98D6C0BA574E9DD6B765F25456882E6082C1429FD4A5EB26AB817B2CEE40C5DD06E15138E3FC1721E7988A35C"  
    decryptionKey="10DE1F68087081848A91B25CCFC8D5E50336D4E081210027"  
    validation="SHA1"  
    decryption="AES" />
```

### Azure Settings
It's suggested that all settings are put on [Azure Configure app settings](https://docs.microsoft.com/en-us/azure/app-service/configure-common#configure-app-settings) as Slot Settings.
| Key                                 | Value                                                             | Comment                     |
| ----------------------------------- | ----------------------------------------------------------------- | --------------------------- |
| UmbracoSlotCopy::ServerToSync       | https://your-production-website/umbraco/wavenet/slotcopy/getfiles | **Required** on Target Slot |
| UmbracoSlotCopy::PathsToSync        | \~/media,\~/css,\~/App_Data/UmbracoForms                          | _Optional_ on Source Slot   |
| UmbracoSlotCopy::FilesToSyncPattern | \*.\*                                                             | _Optional_ on Source Slot   |

### Synchronisation
When everything is set up, you simply make a request on your staging slot: https://your-staging-website/umbraco/wavenet/slotcopy/sync
and it will return the number of items copied from source to target.  
If you run the same on production, it will reject the request with a 404 (check based on `UmbracoSlotCopy::ServerToSync`)

### Integration / Build Server
Just add a step which makes the HTTP request.

## Packages
| **Stable Release**
|-
| [![NuGet](https://img.shields.io/nuget/v/Wavenet.Umbraco7.SlotCopy.svg)](https://www.nuget.org/packages/Wavenet.Umbraco7.SlotCopy)
| **Early Access**
| [![MyGet](https://img.shields.io/myget/wavenet-be/vpre/Wavenet.Umbraco7.SlotCopy.svg)](https://www.myget.org/feed/wavenet-be/package/nuget/Wavenet.Umbraco7.SlotCopy)