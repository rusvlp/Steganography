using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Steganography;

public partial class MainWindow : Window
{

    public Bitmap bitmapOriginal;
    public Bitmap bitmapModified;

    public TextBox PathToLoadBox;
    public TextBox DataToBeHiddenBox;
    public TextBox UnhiddenDataBox;
    public TextBox PathToSaveBox;
    public TextBlock DebugBox;
    
    public MainWindow()
    {
        InitializeComponent();
        PathToLoadBox = this.FindControl<TextBox>("PathToLoad");
        DataToBeHiddenBox = this.FindControl<TextBox>("DataToBeHidden");
        UnhiddenDataBox = this.FindControl<TextBox>("UnhiddenData");
        PathToSaveBox = this.FindControl<TextBox>("PathToSave");
        DebugBox = this.FindControl<TextBlock>("Debug");

    }

    private void LoadImage(object? sender, RoutedEventArgs e)
    {
        try
        {
            bitmapOriginal = (Bitmap)Bitmap.FromFile(PathToLoadBox.Text);
        }
        catch (Exception exception)
        {
            DebugBox.Text = "Произошла ошибка при загрузке изображения: " + exception.Message;
        }
    }


    private void HideMessage(object? sender, RoutedEventArgs e)
    {
        try
        {
            bitmapModified = new Bitmap(
                bitmapOriginal, 
                bitmapOriginal.Width, 
                bitmapOriginal.Height);
        
            int numberbytes = 
                (byte)DataToBeHiddenBox.Text.Length*2;
            byte[] bytesOriginal = new byte[numberbytes+1];

            bytesOriginal[0] = (byte)numberbytes;
            
            Encoding.UTF8.GetBytes(
                DataToBeHiddenBox.Text,
                0,
                DataToBeHiddenBox.Text.Length,
                bytesOriginal,
                1);
            
            int byteCount = 0;
            for (int i = 0; i < bitmapOriginal.Width; i++)
            {
                for (int j = 0; j < bitmapOriginal.Height; j++)
                {
                    if (bytesOriginal.Length == byteCount)
                    {
                        break;
                    }

                    Color clrPixelOriginal = bitmapOriginal.GetPixel(i, j);

                    byte r = (byte)((clrPixelOriginal.R & ~0x7) |
                                    (bytesOriginal[byteCount] >> 0) & 0x7);
                    byte g = 
                        (byte)((clrPixelOriginal.G & ~0x7) |
                               (bytesOriginal[byteCount]>>3)&0x7);
                    byte b = 
                        (byte)((clrPixelOriginal.B & ~0x3) |
                               (bytesOriginal[byteCount]>>6)&0x3);
                
                    bitmapModified.SetPixel(
                        i, j, Color.FromArgb(r, g, b));
                }
            }
        }
        catch (Exception exception)
        {
            DebugBox.Text = "Произошла ошибка при сокрытии: " + exception.Message;
        }
        
        
        
    }

    private void UnhideMessage(object? sender, RoutedEventArgs e)
    {
       

        DebugBox.Text = "UnhideMessage Clicked";
        
        try
        {
            byte[] bytesExtracted = new byte [256+1];
            int byteCount = 0;
            for (int i = 0; i < bitmapModified.Width; i++)
            {
                for (int j = 0; j < bitmapModified.Height; j++)
                {
                    if (byteCount == bytesExtracted.Length)
                    {
                        break;
                    }


                    Color clrPixelModified =
                        bitmapModified.GetPixel(i, j);
                    byte bits123 =
                        (byte)((clrPixelModified.R & 0x7) << 0);
                    byte bits456 = (
                        byte)((clrPixelModified.G & 0x7) << 3);
                    byte bits78 = (
                        byte)((clrPixelModified.B & 0x3) << 6);

                    bytesExtracted[byteCount] =
                        (byte)(bits78 | bits456 | bits123);
                    byteCount++;

                }
            }
            
            int numberbytes = bytesExtracted[0];

            UnhiddenDataBox.Text = Encoding.UTF8.GetString(bytesExtracted, 1, numberbytes);
        }
        catch (Exception exception)
        {
            DebugBox.Text = "Произошла ошибка при извлечении: " + exception.Message;
            
        }
        /*finally
        {
            
        }*/
    }

    private void SaveHandler(object? sender, RoutedEventArgs e)
    {
        string path = PathToSaveBox.Text;
        bitmapModified.Save(path, ImageFormat.Jpeg);
    }
}