﻿using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using LethalConfig;
using BepInEx.Configuration;
using MysteryDice.Dice;
using MysteryDice.Effects;

namespace MysteryDice
{
    internal class ConfigManager
    {
        public static void setupLethalConfig() 
        {
            var minHyperSlider = new FloatSliderConfigItem(MysteryDice.minHyperShake, new FloatSliderOptions
            {
                Min = 0,
                Max = 100
            });
            var maxHyperSlider = new FloatSliderConfigItem(MysteryDice.maxHyperShake, new FloatSliderOptions
            {
                Min = 0,
                Max = 100
            });
            var minNeckSpinSlider = new FloatSliderConfigItem(MysteryDice.minNeckSpin, new FloatSliderOptions
            {
                Min = 0,
                Max = 100
            });
            var rotationSpeedModifierSlider = new FloatSliderConfigItem(MysteryDice.rotationSpeedModifier, new FloatSliderOptions
            {
                Min = 0,
                Max = 100
            });
            var maxNeckSpinSlider = new FloatSliderConfigItem(MysteryDice.maxNeckSpin, new FloatSliderOptions
            {
                Min = 0,
                Max = 100
            });
            var eggExplodeTimeSlider = new FloatSliderConfigItem(MysteryDice.eggExplodeTime, new FloatSliderOptions
            {
                Min = 0,
                Max = 5
            });
            var minNeckBreakTimerSlider = new IntSliderConfigItem(MysteryDice.minNeckBreakTimer, new IntSliderOptions
            {
                Min = 0, 
                Max = 100
            });
            var maxNeckBreakTimerSlider = new IntSliderConfigItem(MysteryDice.maxNeckBreakTimer, new IntSliderOptions
            {
                Min = 0, 
                Max = 100
            });
            
            var EmergencyDiePriceSlider = new IntInputFieldConfigItem(MysteryDice.EmergencyDiePrice, true);
            var EmergencyDieScrapBoolField = new BoolCheckBoxConfigItem(MysteryDice.DieEmergencyAsScrap, true);

            var BlameGlitchMinIntField = new IntInputFieldConfigItem(BlameGlitch.minNum, false);
            var BlameGlitchMaxIntField = new IntInputFieldConfigItem(BlameGlitch.maxNum, false);
            var hypershakeIntField = new IntInputFieldConfigItem(MysteryDice.hyperShakeTimer, false);
            var neckRotationsIntField = new IntInputFieldConfigItem(MysteryDice.neckRotations, false);
            //var rotationSpeedModifierFloatField = new FloatInputFieldConfigItem(MysteryDice.rotationSpeedModifier, false);
            
            var DisplayResultsEnumField = new EnumDropDownConfigItem<DieBehaviour.ShowEffect>(MysteryDice.DisplayResults);
            var DebugChatEnumField = new EnumDropDownConfigItem<MysteryDice.chatDebug>(MysteryDice.debugChat);
            var SizeDifferenceEnumField = new EnumDropDownConfigItem<SizeDifference.sizeRevert>(SizeDifference.sizeOption);
            
            var BlameGlitchInsideBoolField = new BoolCheckBoxConfigItem(BlameGlitch.isInside, false);
            var BlameGlitchbothInsideOutsideBoolField = new BoolCheckBoxConfigItem(BlameGlitch.bothInsideOutside, false);
            var pussyModeBoolField = new BoolCheckBoxConfigItem(MysteryDice.pussyMode, false);
            var GrabDebugBoolField = new BoolCheckBoxConfigItem(MysteryDice.DebugLogging, false);
            var randomSpinTimeBoolField = new BoolCheckBoxConfigItem(MysteryDice.randomSpinTime, true);
            var chronosUpdatedTimeOfDayBoolField = new BoolCheckBoxConfigItem(MysteryDice.chronosUpdatedTimeOfDay, false);
            var useDiceOutsideBoolField = new BoolCheckBoxConfigItem(MysteryDice.useDiceOutside, false);
            var debugDiceBoolField = new BoolCheckBoxConfigItem(MysteryDice.debugDice, false);
            var allowChatCommandsBoolField = new BoolCheckBoxConfigItem(MysteryDice.allowChatCommands, true);
            var useNeckBreakTimerBoolField = new BoolCheckBoxConfigItem(MysteryDice.useNeckBreakTimer, false);
            var debugMenuShowsAllBoolField = new BoolCheckBoxConfigItem(MysteryDice.debugMenuShowsAll, false);
            var DebugButtonBoolField = new BoolCheckBoxConfigItem(MysteryDice.debugButton, true);

            var adminKeybindStringField = new TextInputFieldConfigItem(MysteryDice.adminKeybind, false);



            LethalConfigManager.AddConfigItem(debugDiceBoolField);
            LethalConfigManager.AddConfigItem(adminKeybindStringField);
            LethalConfigManager.AddConfigItem(useDiceOutsideBoolField);
            LethalConfigManager.AddConfigItem(chronosUpdatedTimeOfDayBoolField);
            LethalConfigManager.AddConfigItem(randomSpinTimeBoolField);
            LethalConfigManager.AddConfigItem(pussyModeBoolField);
            LethalConfigManager.AddConfigItem(useNeckBreakTimerBoolField);
            LethalConfigManager.AddConfigItem(SizeDifferenceEnumField);
            LethalConfigManager.AddConfigItem(DisplayResultsEnumField);
            LethalConfigManager.AddConfigItem(minHyperSlider);
            LethalConfigManager.AddConfigItem(maxHyperSlider);
            LethalConfigManager.AddConfigItem(hypershakeIntField);
            LethalConfigManager.AddConfigItem(eggExplodeTimeSlider);
            LethalConfigManager.AddConfigItem(minNeckSpinSlider);
            LethalConfigManager.AddConfigItem(maxNeckSpinSlider);
            LethalConfigManager.AddConfigItem(rotationSpeedModifierSlider);
            LethalConfigManager.AddConfigItem(minNeckBreakTimerSlider);
            LethalConfigManager.AddConfigItem(maxNeckBreakTimerSlider);
            LethalConfigManager.AddConfigItem(neckRotationsIntField);
            LethalConfigManager.AddConfigItem(BlameGlitchInsideBoolField);
            LethalConfigManager.AddConfigItem(BlameGlitchbothInsideOutsideBoolField);
            LethalConfigManager.AddConfigItem(BlameGlitchMinIntField);
            LethalConfigManager.AddConfigItem(BlameGlitchMaxIntField);
            LethalConfigManager.AddConfigItem(debugMenuShowsAllBoolField);
            LethalConfigManager.AddConfigItem(DebugButtonBoolField);
            LethalConfigManager.AddConfigItem(DebugChatEnumField);
            LethalConfigManager.AddConfigItem(GrabDebugBoolField);
            LethalConfigManager.AddConfigItem(EmergencyDiePriceSlider);
            LethalConfigManager.AddConfigItem(EmergencyDieScrapBoolField);

            //if (MysteryDice.lethalThingsPresent)
            //{
            //    var boombaSpeedFloatField = new FloatInputFieldConfigItem(MysteryDice.BoombaEventSpeed, false);
            //    LethalConfigManager.AddConfigItem(boombaSpeedFloatField);
            //}
            foreach (ConfigEntry<bool> config in DieBehaviour.effectConfigs)
            {
                var effectBoolField = new BoolCheckBoxConfigItem(config,true);
                LethalConfigManager.AddConfigItem(effectBoolField);
            }
        }


    }
}
