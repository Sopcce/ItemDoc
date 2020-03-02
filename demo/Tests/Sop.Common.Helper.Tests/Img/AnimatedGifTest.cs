using NUnit.Framework;
using Sop.Common.Helper.Img.Gif;
using System;
using System.IO;

namespace Sop.Common.Helper.Tests.Img
{
    public class AnimatedGifTest
    {
        private string[] imageFilePaths;
        private string outputFilePath;
        private string imageGifPath;
        [SetUp]
        public void Setup()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            imageFilePaths = new String[]
            {
                $"{path}//Resources//01.png",
                $"{path}//Resources//02.png",
                $"{path}//Resources//03.png",
            };

            //ע�⣬����ʹ�ü򵥵�04.gif,��ֻ����3֡��
            //ʹ��05.gif����140֡Ŷ
            imageGifPath = $"{path}//Resources//05.gif";

            outputFilePath = $"{path}//Resources//010203.gif";
        }

        [Test]
        public void Create_Png_Img_To_Gif_Test()
        {
            foreach (var imgFilePath in imageFilePaths)
            {
                if (!File.Exists(imgFilePath))
                {
                    Assert.False(false, "�ļ�·��������");
                }
            }
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
            AnimatedGifEncoder e1 = new AnimatedGifEncoder();
            e1.Start(outputFilePath);
            //e1.Delay = 500;    // �ӳټ��
            e1.SetDelay(500);
            e1.SetRepeat(0);  //-1:��ѭ��,0:����ѭ�� ����  
            e1.SetSize(100, 200);
            foreach (var imgFilePath in imageFilePaths)
            {
                e1.AddFrame(System.DrawingCore.Image.FromFile(imgFilePath));
            }
            e1.Finish();
            var isExists = File.Exists(outputFilePath);
            Assert.IsTrue(isExists, "�ļ����ڣ����ɳɹ�");

        }

        
        [Test]
        public void Create_Gif_Img_To_Png_Test()
        {
            var isExists = File.Exists(imageGifPath);
            Assert.IsTrue(isExists, "�ļ�����");
            AnimatedGifDecoder de = new AnimatedGifDecoder();
            de.Read(imageGifPath);
            for (int i = 0, count = de.GetFrameCount(); i < count; i++)
            {
                System.DrawingCore.Image frame = de.GetFrame(i);

                frame.Save(outputFilePath + Guid.NewGuid().ToString() + ".png");
            }

        }
    }
}