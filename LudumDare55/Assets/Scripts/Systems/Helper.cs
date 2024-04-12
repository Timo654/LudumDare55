using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Helper
{
    // https://discussions.unity.com/t/how-to-convert-from-world-space-to-canvas-space/117981/17
    public static Vector2 WorldToCanvasPosition(Canvas canvas, Camera worldCamera, Vector3 worldPosition)
    {
        //Vector position (percentage from 0 to 1) considering camera size.
        //For example (0,0) is lower left, middle is (0.5,0.5)
        Vector2 viewportPoint = worldCamera.WorldToViewportPoint(worldPosition);

        var rootCanvasTransform = (canvas.isRootCanvas ? canvas.transform : canvas.rootCanvas.transform) as RectTransform;
        var rootCanvasSize = rootCanvasTransform!.rect.size;
        //Calculate position considering our percentage, using our canvas size
        //So if canvas size is (1100,500), and percentage is (0.5,0.5), current value will be (550,250)
        var rootCoord = (viewportPoint - rootCanvasTransform.pivot) * rootCanvasSize;
        if (canvas.isRootCanvas)
            return rootCoord;

        var rootToWorldPos = rootCanvasTransform.TransformPoint(rootCoord);
        return canvas.transform.InverseTransformPoint(rootToWorldPos);
    }
    public static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime.ToString("dd.MM.yy");
    }

    // from https://stackoverflow.com/questions/47143837/resizing-a-list-to-fit-a-new-length
    public static void PaddedResize<T>(List<T> list, int size, T padding = default
)
    {
        // Compute difference between actual size and desired size
        var deltaSize = list.Count - size;

        if (deltaSize < 0)
        {
            // If the list is smaller than target size, fill with `padding`
            list.AddRange(Enumerable.Repeat(padding, -deltaSize));
        }
        else
        {
            // If the list is larger than target size, remove end of list
            list.RemoveRange(size, deltaSize);
        }
    }

    public static ControlType GetControllerType()
    {
        if (BuildConstants.isMobile)
        {
            return ControlType.Mobile;
        }
        else if (Gamepad.current == null)
        {
            return ControlType.Keyboard;
        }
        else if (Gamepad.current.displayName.Contains("DualSense") || Gamepad.current.displayName.Contains("Wireless Controller") || Gamepad.current.displayName.Contains("PS4 Controller") || Gamepad.current.displayName.Contains("PlayStation Controller") || Gamepad.current.displayName.Contains("PS5 Controller"))
        {
            return ControlType.DualShock;
        }
        else
        {
            return ControlType.XBOX;
        }
    }
    public static void ResizeSaveList(List<bool> unlockedItems, int itemCount)
    {
        if (unlockedItems.Count != itemCount)
        {
            PaddedResize(unlockedItems, itemCount, false);
        }
    }
    public static bool CheckAccelerometer()
    {
        if (BuildConstants.isMobile)
        {
            if (Accelerometer.current != null)
            {
                return true;
            }
        }
        else
        {
            // accelerometer is broken in Linux and WebGL for now, so disable the toggle
#if !UNITY_STANDALONE_LINUX && !UNITY_WEBGL
            if (Gamepad.current == null) return false;
            Debug.Log($"Detected gamepad: {Gamepad.current.displayName}");
            if (Gamepad.current.displayName.Contains("DualSense") || Gamepad.current.displayName.Contains("Wireless Controller") || Gamepad.current.displayName.Contains("PS4 Controller") || Gamepad.current.displayName.Contains("PlayStation Controller") || Gamepad.current.displayName.Contains("PS5 Controller")) return true;
#endif
        }
        return false;
    }

    public static Color UIColourToText(Color uiColor)
    {
        return new(uiColor.r * 0.94f, uiColor.g * 0.94f, uiColor.b * 0.94f, uiColor.a);
    }

    public static void Reshuffle<T>(List<T> list)
    {
        // Knuth shuffle algorithm
        for (int t = 0; t < list.Count; t++)
        {
            T tmp = list[t];
            int r = UnityEngine.Random.Range(t, list.Count);
            list[t] = list[r];
            list[r] = tmp;
        }
    }


    //https://stackoverflow.com/questions/4488969/split-a-string-by-capital-letters
    public static string SplitCamelCase(string source)
    {
        return string.Join(" ", Regex.Split(source, @"(?<!^)(?=[A-Z](?![A-Z]|$))"));
    }
}
