using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class FiboManualTrading1H_Indicator : Indicator
    {
        [Parameter("Vertical Alignment", Group = "Position", DefaultValue = VerticalAlignment.Top)]
        public VerticalAlignment VAlignment { get; set; }

        [Parameter("Horizontal Alignment", Group = "Position", DefaultValue = HorizontalAlignment.Right)]
        public HorizontalAlignment HAlignment { get; set; }

        [Parameter("Timeframe", Group = "Timeframe")]
        public TimeFrame DefinedTimeFrame { get; set; }

        [Parameter("Period", Group = "Moving Average", DefaultValue = 200)]
        public int MAPeriod { get; set; }

        [Parameter("MA Type", Group = "Moving Average", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType MAType { get; set; }

        [Parameter("Period", Group = "RSI", DefaultValue = 14)]
        public int RSIPeriod { get; set; }

        private const string UPARROW = "🢁";
        private const string DOWNARROW = "🢃";

        private Box box;
        private string lastToShowText;
        private DateTime? lastOpenTime = null;

        protected override void Initialize()
        {
            Bars definedTimeFrameBars = MarketData.GetBars(DefinedTimeFrame);

            this.box = new Box(DefinedTimeFrame.ShortName, Indicators.MovingAverage(definedTimeFrameBars.ClosePrices, MAPeriod, MAType), Indicators.RelativeStrengthIndex(definedTimeFrameBars.ClosePrices, RSIPeriod));

            lastToShowText = "";
        }

        public override void Calculate(int index)
        {
            int movingAveragesMovement = 0;
            int relativeStrengthIndexMovement = 0;
            double lastClosePrice = Bars.ClosePrices.Last(1);

            if (lastClosePrice > this.box.movingAverage.Result.Last(1))
                movingAveragesMovement = 1;
            else if (lastClosePrice < this.box.movingAverage.Result.Last(1))
                movingAveragesMovement = -1;

            if (this.box.relativeStrengthIndex.Result.Last(1) > 50)
                relativeStrengthIndexMovement = 1;
            else if (this.box.relativeStrengthIndex.Result.Last(1) < 50)
                relativeStrengthIndexMovement = -1;

            string toShowText = this.box.label;
            bool noTrend = false;
            if (movingAveragesMovement > 0 && relativeStrengthIndexMovement > 0)
                toShowText += " " + UPARROW;
            else if (movingAveragesMovement < 0 && relativeStrengthIndexMovement < 0)
                toShowText += " " + DOWNARROW;
            else
            {
                toShowText += " --";
                noTrend = true;
            }

            if (toShowText != lastToShowText)
            {
                Chart.DrawStaticText("FiboManualTrading1H", toShowText, VAlignment, HAlignment, Color.WhiteSmoke);
                lastToShowText = toShowText;
            }

            if (IsLastBar)
            {
                DateTime nowOpenTime = Bars.OpenTimes.LastValue;
                if (lastOpenTime == null)
                    lastOpenTime = nowOpenTime;
                else if (lastOpenTime != nowOpenTime)
                {
                    if (!noTrend)
                    {
                        ShowMessageBox("Indicator: " + toShowText + "\n Current Time: " + DateTime.Now.ToString() + "\n Current TimeFrame: " + TimeFrame.ShortName, Symbol.Name);
                        Notifications.PlaySound("D:\\Documents HDD\\Investment\\forex\\cTraderNotification.mp3");
                    }
                    lastOpenTime = nowOpenTime;
                }

            }

            Print("NoTrend: " + noTrend);
        }

        private void ShowMessageBox(string text, string caption)
        {
            System.Threading.Thread t = new System.Threading.Thread(() => MyMessageBox(text, caption));
            t.Start();
        }

        private void MyMessageBox(object text, object caption)
        {
            System.Windows.Forms.MessageBox.Show((string)text, (string)caption);
        }

        private class Box
        {

            public string label { get; set; }
            public MovingAverage movingAverage { get; set; }
            public RelativeStrengthIndex relativeStrengthIndex { get; set; }

            public Box(string label, MovingAverage movingAverage, RelativeStrengthIndex relativeStrengthIndex)
            {
                this.label = label;
                this.movingAverage = movingAverage;
                this.relativeStrengthIndex = relativeStrengthIndex;
            }

        }
    }
}
