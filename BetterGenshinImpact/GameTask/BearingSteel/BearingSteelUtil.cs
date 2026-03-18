using System;
using System.Text.RegularExpressions;
using BetterGenshinImpact.Core.Recognition.OCR;
using BetterGenshinImpact.Core.Simulator;
using BetterGenshinImpact.Core.Simulator.Extensions;
using BetterGenshinImpact.GameTask.AutoEat.Assets;
using BetterGenshinImpact.GameTask.Common;
using BetterGenshinImpact.GameTask.Common.BgiVision;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using Wpf.Ui.Extensions;

namespace BetterGenshinImpact.GameTask.BearingSteel;
public sealed class BearingSteelUtil
{
    private static readonly ILogger<BearingSteelUtil> Logger = App.GetLogger<BearingSteelUtil>();
    private static DateTime _lastCheckHpDateTime = DateTime.MinValue;

    // 啥也不做为true
    public static bool CheckHp()
    {
        if (!BearingSteelConfig.GetBearingSteelAutoEatEgg())
            return true;
        var now = DateTime.Now;
        if ((now - _lastCheckHpDateTime).TotalMilliseconds > 45)
            _lastCheckHpDateTime = now;
        else
            return true;
        var imageRegion = TaskControl.CaptureToRectArea();

        bool resurrectionIconRaExist = imageRegion.Find(AutoEatAssets.Instance.ResurrectionIconRa).IsExist();

        if (resurrectionIconRaExist)
        {
            Logger.LogInformation("{t}检测到复活料理，尝试按Z复活", DateTime.Now.GetMillisTimestamp());
            Simulation.SendInput.SimulateAction(GIActions.QuickUseGadget);
            return false;
        }

        // 检测到绿色就直接返回吧
        if (imageRegion.SrcMat.At<Vec3b>(1010, 860).Equals(new Vec3b(0x22, 0xD7, 0x96)))
        {
            return true;
        }

        bool recoveryIconRaExist = imageRegion.Find(AutoEatAssets.Instance.RecoveryIconRa).IsExist();

        // 检测血条为红
        if (Bv.CurrentAvatarIsLowHp(imageRegion))
        {
            if (recoveryIconRaExist)
            {
                Logger.LogInformation("检测到血条为红且存在吃药图标，尝试按Z");
                Simulation.SendInput.SimulateAction(GIActions.QuickUseGadget);
                return false;
            }

            Logger.LogInformation("检测到血条为红但不存在吃药图标");
            return true;
        }

        if (!recoveryIconRaExist && !resurrectionIconRaExist)
        {
            return true;
        }

        // 取色方案失败才来识别文字，可能血量只有不到200判断红血失败，或者不在主界面
        var textRect = new Rect(880, 999, 160, 22);
        var textMat = new Mat(imageRegion.SrcMat, textRect);
        Scalar White = new Scalar(255, 255, 255);
        Mat whiteMask = new Mat();
        Cv2.InRange(textMat, White, White, whiteMask);
        Mat blackMask = new Mat();
        Cv2.BitwiseNot(whiteMask, blackMask);
        var text = Regex.Replace(OcrFactory.Paddle.Ocr(blackMask), @"[^0-9/]+", "");
        Match match = Regex.Match(text, @"^(\d+)/(\d+)$");
        if (match.Success)
        {
            uint n1 = uint.Parse(match.Groups[1].Value);
            uint n2 = uint.Parse(match.Groups[2].Value);
            if (n2 < 150000 && n1 <= n2 * 0.3)
            {
                Logger.LogInformation("残血尝试按Z: {text}", text);
                Simulation.SendInput.SimulateAction(GIActions.QuickUseGadget);
            }
        }

        imageRegion.Dispose();
        return false;
    }


    public static bool IsCurrentQ()
    {
        var imageRegion = TaskControl.CaptureToRectArea();
        Rect skillArea = new Rect(1766, 919, 106, 106);
        Mat roi = new Mat(imageRegion.SrcMat, skillArea);

        // 基础参数
        int width = roi.Cols;
        int height = roi.Rows;
        int centerX = width / 2;
        int centerY = height / 2;
        int radiusOuter = Math.Min(width, height) / 2;
        int radiusInner = radiusOuter - 6;
        var indexer = roi.GetGenericIndexer<Vec3b>();

        double[] sumOuter = { 0.0, 0.0, 0.0 };
        double[] sumInner = { 0.0, 0.0, 0.0 };
        int countInner = 0;
        int countOuter = 0;
        var r1 = radiusOuter * radiusOuter + radiusOuter;
        var r2 = radiusOuter * radiusOuter - radiusOuter;
        var r3 = radiusInner * radiusInner + radiusInner;
        var r4 = radiusInner * radiusInner - radiusInner;
        for (int y = 0; y < height; y += 2)
        {
            for (int x = 0; x < width; x += 2)
            {
                int distSq = (x - centerX) * (x - centerX) + (y - centerY) * (y - centerY);

                if (distSq <= r1 && distSq >= r2)
                {
                    countOuter++;
                    sumOuter[0] += indexer[y, x].Item0;
                    sumOuter[1] += indexer[y, x].Item1;
                    sumOuter[2] += indexer[y, x].Item2;
                }

                if (distSq <= r3 && distSq >= r4)
                {
                    countInner++;
                    sumInner[0] += indexer[y, x].Item0;
                    sumInner[1] += indexer[y, x].Item1;
                    sumInner[2] += indexer[y, x].Item2;
                }
            }
        }

        double rate1 = (sumOuter[1] * countInner) / (sumInner[1] * countOuter);
        double rate2 = (sumOuter[2] * countInner) / (sumInner[2] * countOuter);
        double rate0 = (sumOuter[0] * countInner) / (sumInner[0] * countOuter);
        if (rate0 > 1 && rate1 > 1 && rate2 > 1)
        {
            var textRect = new Rect(1780, 957, 80, 30);
            var textMat = new Mat(imageRegion.SrcMat, textRect);
            var text = OcrFactory.Paddle.Ocr(textMat);
            return !Regex.IsMatch(text, @"\d");
        }

        if (rate0 < 1 && rate1 < 1 && rate2 < 1)
        {
            return false;
        }

        Logger.LogInformation("鉴定当前角色Q状态失败 {x} {a} {b} ", rate0, rate1, rate2);
        return false;
    }
}