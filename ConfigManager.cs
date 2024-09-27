using LethalConfig.ConfigItems;
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

            var neckRotationsIntField = new IntInputFieldConfigItem(MysteryDice.neckRotations, false);
            var rotationSpeedModifierFloatField = new FloatInputFieldConfigItem(MysteryDice.rotationSpeedModifier, false);
            
            var DisplayResultsEnumField = new EnumDropDownConfigItem<DieBehaviour.ShowEffect>(MysteryDice.DisplayResults);
            var SizeDifferenceEnumField = new EnumDropDownConfigItem<SizeDifference.sizeRevert>(SizeDifference.sizeOption);
            
            var pussyModeBoolField = new BoolCheckBoxConfigItem(MysteryDice.pussyMode, false);
            var randomSpinTimeBoolField = new BoolCheckBoxConfigItem(MysteryDice.randomSpinTime, true);
            var chronosUpdatedTimeOfDayBoolField = new BoolCheckBoxConfigItem(MysteryDice.chronosUpdatedTimeOfDay, false);
            var useDiceOutsideBoolField = new BoolCheckBoxConfigItem(MysteryDice.useDiceOutside, false);
            var debugDiceBoolField = new BoolCheckBoxConfigItem(MysteryDice.debugDice, false);
            var allowChatCommandsBoolField = new BoolCheckBoxConfigItem(MysteryDice.allowChatCommands, true);
            var useNeckBreakTimerBoolField = new BoolCheckBoxConfigItem(MysteryDice.useNeckBreakTimer, false);
            var DebugButtonBoolField = new BoolCheckBoxConfigItem(MysteryDice.debugButton, true);

            var adminKeybindStringField = new TextInputFieldConfigItem(MysteryDice.adminKeybind, false);

            LethalConfigManager.AddConfigItem(allowChatCommandsBoolField);
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
            LethalConfigManager.AddConfigItem(minNeckSpinSlider);
            LethalConfigManager.AddConfigItem(maxNeckSpinSlider);
            LethalConfigManager.AddConfigItem(rotationSpeedModifierSlider);
            LethalConfigManager.AddConfigItem(minNeckBreakTimerSlider);
            LethalConfigManager.AddConfigItem(maxNeckBreakTimerSlider);
            LethalConfigManager.AddConfigItem(neckRotationsIntField);
            LethalConfigManager.AddConfigItem(rotationSpeedModifierFloatField);
            LethalConfigManager.AddConfigItem(DebugButtonBoolField);

            foreach(ConfigEntry<bool> config in DieBehaviour.effectConfigs)
            {
                var effectBoolField = new BoolCheckBoxConfigItem(config,true);
                LethalConfigManager.AddConfigItem(effectBoolField);
            }
        }


    }
}
