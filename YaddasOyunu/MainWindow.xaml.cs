using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace YaddasOyunu
{
    public partial class MainWindow : Window
    {
        // 4 rəng (hər biri 2 dəfə -> cəmi 8 kart)
        private readonly Brush[] palette =
        {
            Brushes.Red,
            Brushes.DodgerBlue,
            Brushes.Green,
            Brushes.Gold
        };

        private readonly Brush closedBrush = new SolidColorBrush(Color.FromRgb(0xE6, 0xE6, 0xE6));

        private Button[] cards;
        private Brush[] cardColors = new Brush[8];   // hər kartın gizli rəngi
        private bool[] matched = new bool[8];        // tapılmış kartlar

        private Button firstCard = null;
        private Button secondCard = null;
        private bool waiting = false;                // səhv cütlük göstərilərkən kliklər bloklanır
        private int score = 0;

        private readonly DispatcherTimer hideTimer = new DispatcherTimer();
        private readonly Random rnd = new Random();

        public MainWindow()
        {
            InitializeComponent();

            cards = new[] { Card0, Card1, Card2, Card3, Card4, Card5, Card6, Card7 };

            hideTimer.Interval = TimeSpan.FromMilliseconds(900);
            hideTimer.Tick += HideTimer_Tick;

            StartNewGame();
        }

        // ---------- Yeni oyun ----------
        private void StartNewGame()
        {
            hideTimer.Stop();
            waiting = false;
            firstCard = null;
            secondCard = null;
            score = 0;
            ScoreText.Text = "0";

            // 4 rəng x 2 = 8 element, qarışdır
            List<Brush> list = palette.Concat(palette).OrderBy(_ => rnd.Next()).ToList();

            for (int i = 0; i < cards.Length; i++)
            {
                cardColors[i] = list[i];
                matched[i] = false;
                cards[i].Background = closedBrush;
                cards[i].IsEnabled = true;
            }

            OkIcon.Opacity = 1;
            WrongIcon.Opacity = 1;
        }

        // ---------- Karta klik ----------
        private void Card_Click(object sender, RoutedEventArgs e)
        {
            if (waiting) return;

            Button btn = (Button)sender;
            int index = int.Parse(btn.Tag.ToString());

            if (matched[index] || btn == firstCard) return;

            // kartı aç
            btn.Background = cardColors[index];

            if (firstCard == null)
            {
                firstCard = btn;
                return;
            }

            secondCard = btn;
            int i1 = int.Parse(firstCard.Tag.ToString());
            int i2 = index;

            if (ReferenceEquals(cardColors[i1], cardColors[i2]))
            {
                // ✅ Düz təxmin -> xal artır
                matched[i1] = true;
                matched[i2] = true;
                score++;
                ScoreText.Text = score.ToString();

                OkIcon.Opacity = 1;
                WrongIcon.Opacity = 0.2;

                firstCard = null;
                secondCard = null;

                CheckGameOver();
            }
            else
            {
                // ❌ Səhv təxmin -> xal dəyişmir
                OkIcon.Opacity = 0.2;
                WrongIcon.Opacity = 1;

                waiting = true;
                hideTimer.Start();
            }
        }

        // Səhv cütlüyü bir az göstərib bağla
        private void HideTimer_Tick(object sender, EventArgs e)
        {
            hideTimer.Stop();

            firstCard.Background = closedBrush;
            secondCard.Background = closedBrush;

            firstCard = null;
            secondCard = null;
            waiting = false;
        }

        // ---------- Oyunun sonu ----------
        private void CheckGameOver()
        {
            if (matched.All(m => m))
            {
                MessageBox.Show("Bütün qutular açıldı. Oyun bitdi!\nXalınız: " + score + " / 4",
                                "Oyun bitdi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e) => StartNewGame();

        private void Exit_Click(object sender, RoutedEventArgs e) => Close();
    }
}
