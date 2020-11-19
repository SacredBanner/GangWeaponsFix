using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.Issues;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace GangWeaponsFix
{
    public class GangWeaponsFixSubModule : MBSubModuleBase
    {
        private static Harmony harmonyInstance = new Harmony("GangWeaponsFix.Harmony");

        public override void OnCampaignStart(Game game, object starterObject)
        {
            base.OnCampaignStart(game, starterObject);

            Debug.Print("GangWeaponsFix.OnCampaignStart");
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);
            Debug.Print("GangWeaponsFix.OnGameEnd");
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Debug.Print("GangWeaponsFix.OnSubModuleLoad");

            harmonyInstance.PatchAll();
        }
    }


    // Fix the issue where the "Gang Leader Needs Weapons" quest removes weapons of any type from the player inventory instead of just the requested type
    // Will also order by value and take the cheapest weapons first, so the player doesn't lose expensive axes
    // Since GangLeaderNeedsWeaponsIssueQuest is an internal class, we can't access the type directly. Need to use reflection.
    [HarmonyPatch()]
    public class GangLeaderNeedsWeaponsFixPatch
    {
        static MethodInfo TargetMethod()
        {
            // Get the target method via reflection
            return AccessTools.Method(typeof(GangLeaderNeedsWeaponsIssueQuestBehavior).GetNestedType("GangLeaderNeedsWeaponsIssueQuest", BindingFlags.NonPublic | BindingFlags.Instance), "QuestSuccessDeleteWeaponsFromPlayer");
        }

        // Compares item base value for sorting
        private static int CompareItemValue(ItemRosterElement element1, ItemRosterElement element2)
        {
            if (element1.EquipmentElement.GetBaseValue() < element2.EquipmentElement.GetBaseValue())
            {
                return -1;
            }
            else if (element1.EquipmentElement.GetBaseValue() == element2.EquipmentElement.GetBaseValue())
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        static bool Prefix(Object __instance)
        {
            // Need this type to use reflection to retrieve the field values
            Type QuestBehaviorType = typeof(GangLeaderNeedsWeaponsIssueQuestBehavior).GetNestedType("GangLeaderNeedsWeaponsIssueQuest", BindingFlags.NonPublic | BindingFlags.Instance);

            // Get the field values we need via reflection
            int requestedWeaponAmount = (int)QuestBehaviorType.GetField("_requestedWeaponAmount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            WeaponClass requestedWeaponClass = (WeaponClass)QuestBehaviorType.GetField("_requestedWeaponClass", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            ItemRosterElement[] copyOfAllElements = PartyBase.MainParty.ItemRoster.GetCopyOfAllElements();

            List<ItemRosterElement> requestedWeapons = new List<ItemRosterElement>();

            // Only consider weapons of the requested class
            foreach (ItemRosterElement itemRosterElement in copyOfAllElements)
            {
                if (itemRosterElement.EquipmentElement.Item != null)
                {
                    if (itemRosterElement.EquipmentElement.Item.WeaponComponent != null)
                    {
                        if (itemRosterElement.EquipmentElement.Item.WeaponComponent.PrimaryWeapon.WeaponClass == requestedWeaponClass)
                        {
                            requestedWeapons.Add(itemRosterElement);
                        }
                    }
                }
            }

            // Sort the weapons by increasing value so that we take the cheapest ones first
            requestedWeapons.Sort(CompareItemValue);

            // Remove the weapons.
            // Since we've sorted by increasing value, we will remove the cheapest weapons first.
            foreach (ItemRosterElement itemRosterElement in requestedWeapons)
            {
                if (itemRosterElement.EquipmentElement.Item != null && (itemRosterElement.EquipmentElement.Item.WeaponComponent != null && itemRosterElement.Amount > 0))
                {
                    if (requestedWeaponAmount >= itemRosterElement.Amount)
                    {
                        PartyBase.MainParty.ItemRoster.AddToCounts(itemRosterElement.EquipmentElement, -itemRosterElement.Amount, true);
                        requestedWeaponAmount -= itemRosterElement.Amount;
                    }
                    else
                    {
                        PartyBase.MainParty.ItemRoster.AddToCounts(itemRosterElement.EquipmentElement, -requestedWeaponAmount, true);
                        break;
                    }
                }
            }

            // Returning false will skip the original function complately
            // This is completely overriding behavior of the original function
            // If another mod touched this function, it will cause incompatibility
            return false;
        }
    }

}


