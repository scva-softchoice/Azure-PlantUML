// From Rich Newman
// https://richnewman.wordpress.com/about/code-listings-and-diagrams/hslcolor-class/
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

/// <summary>
/// Represents a color in HSL (Hue, Saturation, Luminosity) format.
/// </summary>
public class HSLColor
{
    // Private data members below are on scale 0-1
    // They are scaled for use externally based on scale
    private double hue = 1.0;
    private double saturation = 1.0;
    private double luminosity = 1.0;

    private const double scale = 240.0;

    /// <summary>
    /// Gets or sets the hue component of the color.
    /// </summary>
    public double Hue
    {
        get { return hue * scale; }
        set { hue = CheckRange(value / scale); }
    }

    /// <summary>
    /// Gets or sets the saturation component of the color.
    /// </summary>
    public double Saturation
    {
        get { return saturation * scale; }
        set { saturation = CheckRange(value / scale); }
    }

    /// <summary>
    /// Gets or sets the luminosity component of the color.
    /// </summary>
    public double Luminosity
    {
        get { return luminosity * scale; }
        set { luminosity = CheckRange(value / scale); }
    }

    private double CheckRange(double value)
    {
        if (value < 0.0)
            value = 0.0;
        else if (value > 1.0)
            value = 1.0;
        return value;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return String.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
    }

    /// <summary>
    /// Returns a string representation of the color in RGB format.
    /// </summary>
    /// <returns>A string representation of the color in RGB format.</returns>
    public string ToRGBString()
    {
        Color color = (Color)this;
        return String.Format("R: {0:#0.##} G: {1:#0.##} B: {2:#0.##}", color.R, color.G, color.B);
    }

    #region Casts to/from System.Drawing.Color
    /// <summary>
    /// Converts an HSLColor to a System.Drawing.Color.
    /// </summary>
    /// <param name="hslColor">The HSLColor to convert.</param>
    public static implicit operator Color(HSLColor hslColor)
    {
        double r = 0, g = 0, b = 0;
        if (hslColor.luminosity != 0)
        {
            if (hslColor.saturation == 0)
                r = g = b = hslColor.luminosity;
            else
            {
                double temp2 = GetTemp2(hslColor);
                double temp1 = 2.0 * hslColor.luminosity - temp2;

                r = GetColorComponent(temp1, temp2, hslColor.hue + 1.0 / 3.0);
                g = GetColorComponent(temp1, temp2, hslColor.hue);
                b = GetColorComponent(temp1, temp2, hslColor.hue - 1.0 / 3.0);
            }
        }
        return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
    }

    private static double GetColorComponent(double temp1, double temp2, double temp3)
    {
        temp3 = MoveIntoRange(temp3);
        if (temp3 < 1.0 / 6.0)
            return temp1 + (temp2 - temp1) * 6.0 * temp3;
        else if (temp3 < 0.5)
            return temp2;
        else if (temp3 < 2.0 / 3.0)
            return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
        else
            return temp1;
    }
    private static double MoveIntoRange(double temp3)
    {
        if (temp3 < 0.0)
            temp3 += 1.0;
        else if (temp3 > 1.0)
            temp3 -= 1.0;
        return temp3;
    }
    private static double GetTemp2(HSLColor hslColor)
    {
        double temp2;
        if (hslColor.luminosity < 0.5)  //<=??
            temp2 = hslColor.luminosity * (1.0 + hslColor.saturation);
        else
            temp2 = hslColor.luminosity + hslColor.saturation - (hslColor.luminosity * hslColor.saturation);
        return temp2;
    }

    /// <summary>
    /// Converts a System.Drawing.Color to an HSLColor.
    /// </summary>
    /// <param name="color">The System.Drawing.Color to convert.</param>
    public static implicit operator HSLColor(Color color)
    {
        HSLColor hslColor = new HSLColor();
        hslColor.hue = color.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
        hslColor.luminosity = color.GetBrightness();
        hslColor.saturation = color.GetSaturation();
        return hslColor;
    }
    #endregion

    /// <summary>
    /// Sets the RGB components of the color.
    /// </summary>
    /// <param name="red">The red component.</param>
    /// <param name="green">The green component.</param>
    /// <param name="blue">The blue component.</param>
    public void SetRGB(int red, int green, int blue)
    {
        HSLColor hslColor = (HSLColor)Color.FromArgb(red, green, blue);
        this.hue = hslColor.hue;
        this.saturation = hslColor.saturation;
        this.luminosity = hslColor.luminosity;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HSLColor"/> class.
    /// </summary>
    public HSLColor() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="HSLColor"/> class from a System.Drawing.Color.
    /// </summary>
    /// <param name="color">The System.Drawing.Color to initialize from.</param>
    public HSLColor(Color color)
    {
        SetRGB(color.R, color.G, color.B);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HSLColor"/> class from RGB components.
    /// </summary>
    /// <param name="red">The red component.</param>
    /// <param name="green">The green component.</param>
    /// <param name="blue">The blue component.</param>
    public HSLColor(int red, int green, int blue)
    {
        SetRGB(red, green, blue);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HSLColor"/> class from HSL components.
    /// </summary>
    /// <param name="hue">The hue component.</param>
    /// <param name="saturation">The saturation component.</param>
    /// <param name="luminosity">The luminosity component.</param>
    public HSLColor(double hue, double saturation, double luminosity)
    {
        this.Hue = hue;
        this.Saturation = saturation;
        this.Luminosity = luminosity;
    }

}
