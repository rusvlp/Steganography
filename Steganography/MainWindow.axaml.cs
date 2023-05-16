using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace Steganography;

public partial class MainWindow : Window
{

    /*public Bitmap bitmapOriginal;
    public Bitmap bitmapModified; */

    public Image<Rgb24> ImageOriginal;
    public Image<Rgb24> ImageModified;
    
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

        //FromImageToByteArrayBitmap();

    }

    public void FromImageToByteArrayBitmap()
    {
        ImageOriginal = (Image<Rgb24>)Image.Load("/Users/Vlad/Documents/ib/Steganography/1200px-Sunflower_from_Silesia2.jpeg");
        MemoryStream ms = new MemoryStream(); 
        ImageOriginal.Save(ms, new JpegEncoder());
        byte[] originalBytes = ms.ToArray();
        DebugBox.Text = Encoding.UTF8.GetString(originalBytes);
        
    }

    private void LoadImage(object? sender, RoutedEventArgs e)
    {
        string path = PathToLoadBox.Text;
        
        ImageOriginal = (Image<Rgb24>)Image.Load(path);
        
        
        
        
    }

    private void HideMessage(object? sender, RoutedEventArgs e)
    {
        
        
        
        string data = DataToBeHiddenBox.Text;

        //Перевод из строки в массив byte
        byte[] messageBytes = Encoding.UTF8.GetBytes(data);

        
        MemoryStream ms = new MemoryStream();
        ImageOriginal.Save(ms, new JpegEncoder());
        byte[] imageBytes = ms.ToArray();
        byte[] modifiedImageBytes = new byte[imageBytes.Length];
        //Создание массива byte[], первый элемент которого - длина сообщения (ограничение сообщения - 256 символов)
      //  byte[] additionalImageBytes = new byte[imageBytes.Length + 1];
       // Array.Copy(imageBytes, 0, additionalImageBytes, 1, imageBytes.Length+1);
        //additionalImageBytes[0] = (byte)messageBytes.Length;

        BitArray baR = Util.Functions.ByteToBit(imageBytes[0]);
        BitArray baG = Util.Functions.ByteToBit(imageBytes[1]);
        BitArray baB = Util.Functions.ByteToBit(imageBytes[2]);

        byte size = (byte)messageBytes.Length;
        BitArray baSize = Util.Functions.ByteToBit(size);

        // Изменяем первый байт изображения так, чтобы он содержал информацию о размере изображения
        baR[0] = baSize[0];
        baR[1] = baSize[1];
        baR[2] = baSize[2];

        baG[0] = baSize[3];
        baG[1] = baSize[4];
        baG[2] = baSize[5];

        baB[0] = baSize[6];
        baB[1] = baSize[7];

        modifiedImageBytes[0] = Util.Functions.BitToByte(baR);
        modifiedImageBytes[1] = Util.Functions.BitToByte(baG);
        modifiedImageBytes[2] = Util.Functions.BitToByte(baB);

        // i - счетчик для messageBytes
        // j - счетчик для modifiedImageBytes
        BitArray baOriginal;
        for (int i = 0, j = 2; i < messageBytes.Length; i++, j += 3)
        {
            baR = Util.Functions.ByteToBit(imageBytes[j]);
            baG = Util.Functions.ByteToBit(imageBytes[j+1]);
            baB = Util.Functions.ByteToBit(imageBytes[j+2]);

            baOriginal = Util.Functions.ByteToBit(messageBytes[i]);

            baR[0] = baOriginal[0];
            baR[1] = baOriginal[1];
            baR[2] = baOriginal[2];

            baG[0] = baOriginal[3];
            baG[1] = baOriginal[4];
            baG[2] = baOriginal[5];

            baB[0] = baOriginal[6];
            baB[1] = baOriginal[7];

            modifiedImageBytes[j]= Util.Functions.BitToByte(baR);
            modifiedImageBytes[j + 1] = Util.Functions.BitToByte(baG);
            modifiedImageBytes[j + 2] = Util.Functions.BitToByte(baB);
            
        }

        MemoryStream ms2 = new MemoryStream(modifiedImageBytes);
        ImageModified = (Image<Rgb24>)Image.Load(ms2);
        
    }

    private void UnhideMessage(object? sender, RoutedEventArgs e)
    {
        MemoryStream ms = new MemoryStream();
        ImageModified.Save(ms, new JpegEncoder());

        byte[] modifiedBytes = ms.ToArray();
        
        // Извлечение размера сообщения

        BitArray baR = Util.Functions.ByteToBit(modifiedBytes[0]);
        BitArray baG = Util.Functions.ByteToBit(modifiedBytes[1]);
        BitArray baB = Util.Functions.ByteToBit(modifiedBytes[2]);

        BitArray size = new BitArray(8);
        size[0] = baR[0];
        size[1] = baR[1];
        size[2] = baR[2];

        size[3] = baG[0];
        size[4] = baG[1];
        size[5] = baG[2];

        size[6] = baB[0];
        size[7] = baB[1];

        byte sizeValue = Util.Functions.BitToByte(size);

        
        // i - счетчик для size
        // j - счетчик для modifiedBytes

        
        byte[] message = new byte[sizeValue];
        byte currentByte = 0;

        BitArray currentBitArray;
        for (int i = 0, j = 2; i < (int)sizeValue; i++, j += 3)
        {
            baR = Util.Functions.ByteToBit(modifiedBytes[j]);
            baG = Util.Functions.ByteToBit(modifiedBytes[j+1]);
            baB = Util.Functions.ByteToBit(modifiedBytes[j+2]);

            currentBitArray = Util.Functions.ByteToBit(currentByte);

            currentBitArray[0] = baR[0];
            currentBitArray[1] = baR[1];
            currentBitArray[2] = baR[2];

            currentBitArray[3] = baG[0];
            currentBitArray[4] = baG[1];
            currentBitArray[5] = baG[2];

            currentBitArray[6] = baB[0];
            currentBitArray[7] = baB[1];

            currentByte = Util.Functions.BitToByte(currentBitArray);
            message[i] = currentByte;
        }

        DebugBox.Text = Encoding.UTF8.GetString(message);
    }

    private void SaveHandler(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
    
    
   /* private void LoadImage(object? sender, RoutedEventArgs e)
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
            
        }
    }

    private void SaveHandler(object? sender, RoutedEventArgs e)
    {
        string path = PathToSaveBox.Text;
        bitmapModified.Save(path, ImageFormat.Jpeg);
    } */
  
}