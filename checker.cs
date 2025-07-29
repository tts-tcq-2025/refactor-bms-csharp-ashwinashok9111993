using System;
using System.Diagnostics;

class Checker
{
    public static bool VitalsOk(float temperature, int pulseRate, int spo2)
    {
        if (!IsTemperatureOk(temperature))
        {
            DisplayCriticalAlert("Temperature critical!");
            return false;
        }
        if (!IsPulseRateOk(pulseRate))
        {
            DisplayCriticalAlert("Pulse Rate is out of range!");
            return false;
        }
        if (!IsSpo2Ok(spo2))
        {
            DisplayCriticalAlert("Oxygen Saturation out of range!");
            return false;
        }

        Console.WriteLine("Vitals received within normal range");
        Console.WriteLine("Temperature: {0} Pulse: {1}, SO2: {2}", temperature, pulseRate, spo2);
        return true;
    }

    private static bool IsTemperatureOk(float temperature)
    {
        return temperature >= 95 && temperature <= 102;
    }

    private static bool IsPulseRateOk(int pulseRate)
    {
        return pulseRate >= 60 && pulseRate <= 100;
    }

    private static bool IsSpo2Ok(int spo2)
    {
        return spo2 >= 90;
    }

    private static void DisplayCriticalAlert(string message)
    {
        Console.WriteLine(message);
        for (int i = 0; i < 6; i++)
        {
            Console.Write("\r* ");
            System.Threading.Thread.Sleep(1000);
            Console.Write("\r *");
            System.Threading.Thread.Sleep(1000);
        }
    }
}
