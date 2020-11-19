# GangWeaponsFix
A bugfix mod for Mount & Blade 2: Bannerlord. Fixes the bug with the "Gang Leader Needs Weapons" quest where weapons of any type are taken instead of just the requested type (axes only as of 1.5.3/1.5.4 beta). 

With this fix, only axes will be taken from the player's inventory, and it will take the cheapest axes first (in case you have an expensive axe your don't want to lose). The original behavior took weapons in the order they appeared in the inventory data, which could take expensive items before cheaper ones.

# Compatibility
This should work for Bannerlord versions 1.5.3 and 1.5.4 beta.

This mod will conflict with any other mod affecting the function GangLeaderNeedsWeaponsIssueQuestBehavior.GangLeaderNeedsWeaponsIssueQuest.QuestSuccessDeleteWeaponsFromPlayer, as I have completely overriden the behavior with a harmony prefix that skips the original function.

# Setup

If you want to compile locally, you will need to update the C# project references for Harmony (2.0.4.0) and the following Bannerlord dlls in steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client:

* TalesWorlds.CampaignSystem
* TalesWorlds.Core
* TalesWorlds.Library
* TalesWorlds.MountAndBlade
* TalesWorlds.ObjectSystem

# Credits
Harmony: https://github.com/pardeike/Harmony
