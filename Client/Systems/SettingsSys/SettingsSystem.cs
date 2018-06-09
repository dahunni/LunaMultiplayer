﻿using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Network;
using LunaCommon.Enums;
using System;
using System.Text;

namespace LunaClient.Systems.SettingsSys
{
    public class SettingsSystem : MessageSystem<SettingsSystem, SettingsMessageSender, SettingsMessageHandler>
    {
        public static SettingStructure CurrentSettings { get; }
        public static SettingsServerStructure ServerSettings { get; private set; } = new SettingsServerStructure();

        private static readonly StringBuilder Builder = new StringBuilder();

        public override string SystemName { get; } = nameof(SettingsSystem);

        static SettingsSystem() => CurrentSettings = SettingsReadSaveHandler.ReadSettings();
        
        protected override void OnDisabled()
        {
            base.OnDisabled();
            ServerSettings = new SettingsServerStructure();
        }

        public static void SaveSettings()
        {
            SettingsReadSaveHandler.SaveSettings(CurrentSettings);
        }

        public static bool ValidateSettings()
        {
            Builder.Length = 0;
            var validationResult = true;

            if ((int) ServerSettings.TerrainQuality != PQSCache.PresetList.presetIndex)
            {
                validationResult = false;
                Builder.Append($"Your terrain quality ({((TerrainQuality)PQSCache.PresetList.presetIndex).ToString()}) " +
                                     $"does not match the server quality ({ServerSettings.TerrainQuality.ToString()}).");

            }

            if (!validationResult)
            {
                NetworkConnection.Disconnect(Builder.ToString());
            }

            return validationResult;
        }

        /// <summary>
        /// Here we can adjust local settings to what we received from the server
        /// </summary>
        public void AdjustLocalSettings()
        {
            //Increase the interpolation offset if necessary
            var minRecommendedInterpolationOffset = TimeSpan.FromMilliseconds(ServerSettings.SecondaryVesselUpdatesMsInterval * 4).TotalSeconds;
            if (CurrentSettings.InterpolationOffsetSeconds < minRecommendedInterpolationOffset)
            {
                LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.IncreasedInterpolationOffset, 30, ScreenMessageStyle.UPPER_RIGHT);
                LunaLog.LogWarning(LocalizationContainer.ScreenText.IncreasedInterpolationOffset);
                CurrentSettings.InterpolationOffsetSeconds = minRecommendedInterpolationOffset;
            }
        }
    }
}
