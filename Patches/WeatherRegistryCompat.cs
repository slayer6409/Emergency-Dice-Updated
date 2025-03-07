using System.Collections.Generic;
using UnityEngine;
using WeatherRegistry;

namespace MysteryDice.Patches;

public class WeatherRegistryCompat : MonoBehaviour
{
    public static List<string> getWeathers()
    {
        List<string> weatherNames = new List<string>();
        foreach (var weather in WeatherRegistry.WeatherManager.Weathers)
        {
            weatherNames.Add(weather.name);
        } 
        return weatherNames;
    }
    public static void setWeather(string weather)
    {
        WeatherRegistry.WeatherController.ChangeCurrentWeather(WeatherRegistry.WeatherManager.Weathers.Find(x => x.name == weather));
    }
    public static void addWeather(string weather)
    {
        if(StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.None)  
            WeatherRegistry.WeatherController.ChangeCurrentWeather(WeatherRegistry.WeatherManager.Weathers.Find(x => x.name == weather));
        else
            WeatherRegistry.WeatherController.AddWeatherEffect(WeatherRegistry.WeatherManager.Weathers.Find(x => x.name == weather));
    }  
    
    
}
