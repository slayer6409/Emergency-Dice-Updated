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
        public static void addConfig(ConfigEntry<bool> config)
        {
            var addCongigField = new BoolCheckBoxConfigItem(config, true);
            LethalConfigManager.AddConfigItem(addCongigField);

        }
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
            //var DisableSizeBasedBoolField = new BoolCheckBoxConfigItem(MysteryDice.DisableSizeBased, true);
            var BetterDebugMenuBoolField = new BoolCheckBoxConfigItem(MysteryDice.BetterDebugMenu, false);
            var BlameGlitchMeteorsBoolField = new BoolCheckBoxConfigItem(BlameGlitch.GlitchedMeteorShower, false);
            var AlarmBoolField = new BoolCheckBoxConfigItem(AlarmCurse.fireAlarm, false);
            var AlarmHorribleBoolField = new BoolCheckBoxConfigItem(AlarmCurse.HorribleVersion, false);
            var doDiceExplosionBoolField = new BoolCheckBoxConfigItem(MysteryDice.doDiceExplosion, false);
            var LoversStartBoolField = new BoolCheckBoxConfigItem(MysteryDice.LoversOnStart, false);
            var DebugMenuClosesAfterBoolField = new BoolCheckBoxConfigItem(MysteryDice.DebugMenuClosesAfter, false);

            //var adminKeybindStringField = new TextInputFieldConfigItem(MysteryDice.adminKeybind, false);


            LethalConfigManager.AddConfigItem(debugDiceBoolField);
            //LethalConfigManager.AddConfigItem(adminKeybindStringField);
            LethalConfigManager.AddConfigItem(useDiceOutsideBoolField);
            LethalConfigManager.AddConfigItem(BetterDebugMenuBoolField);
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
            LethalConfigManager.AddConfigItem(DebugMenuClosesAfterBoolField);
            LethalConfigManager.AddConfigItem(minNeckSpinSlider);
            LethalConfigManager.AddConfigItem(maxNeckSpinSlider);
            LethalConfigManager.AddConfigItem(rotationSpeedModifierSlider);
            LethalConfigManager.AddConfigItem(minNeckBreakTimerSlider);
            LethalConfigManager.AddConfigItem(maxNeckBreakTimerSlider);
            LethalConfigManager.AddConfigItem(neckRotationsIntField);
            LethalConfigManager.AddConfigItem(BlameGlitchInsideBoolField);
            LethalConfigManager.AddConfigItem(BlameGlitchbothInsideOutsideBoolField);
            LethalConfigManager.AddConfigItem(BlameGlitchMinIntField);
            //LethalConfigManager.AddConfigItem(DisableSizeBasedBoolField);
            LethalConfigManager.AddConfigItem(BlameGlitchMaxIntField);
            LethalConfigManager.AddConfigItem(debugMenuShowsAllBoolField);
            LethalConfigManager.AddConfigItem(DebugButtonBoolField);
            LethalConfigManager.AddConfigItem(DebugChatEnumField);
            LethalConfigManager.AddConfigItem(GrabDebugBoolField);
            LethalConfigManager.AddConfigItem(EmergencyDiePriceSlider);
            LethalConfigManager.AddConfigItem(LoversStartBoolField);
            LethalConfigManager.AddConfigItem(EmergencyDieScrapBoolField);
            LethalConfigManager.AddConfigItem(doDiceExplosionBoolField);
            LethalConfigManager.AddConfigItem(BlameGlitchMeteorsBoolField);
            LethalConfigManager.AddConfigItem(AlarmBoolField);
            LethalConfigManager.AddConfigItem(AlarmHorribleBoolField);

            if (MysteryDice.SurfacedPresent)
            {
                var BeybladeBoolField = new BoolCheckBoxConfigItem(Flinger.beybladeMode, false);
                LethalConfigManager.AddConfigItem(BeybladeBoolField);
            }
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
            // foreach (ConfigEntry<bool> config in DieBehaviour.favConfigs)
            // {
            //     var effectBoolField = new BoolCheckBoxConfigItem(config,false);
            //     LethalConfigManager.AddConfigItem(effectBoolField);
            // }
        }
    }
}
