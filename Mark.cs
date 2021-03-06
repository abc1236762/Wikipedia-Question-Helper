﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DataLabelingHelper
{
	public static class Mark
	{
		public static SolidColorBrush InlineBackgroundBrush = new SolidColorBrush(Color.FromArgb(0xBF, 0xFF, 0x00, 0x00));
		public static SolidColorBrush AnswerForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x1E, 0x1E, 0x1E));
		public static SolidColorBrush AnswerBackgroundBrush = new SolidColorBrush(Color.FromArgb(0x9F, 0xDC, 0xDC, 0xDC));
		public static SolidColorBrush TaggedForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xDC, 0xDC, 0xDC));
		public static SolidColorBrush HighLineBackgroundBrush = new SolidColorBrush(Color.FromArgb(0x9F, 0x9C, 0x9C, 0x9C));

		public static bool CheckAnswer(string Context, string Question, string Answer,
			int AnswerNumber, int Number, ref string Answers, ref int AnswerCount) {
			if (Question != string.Empty) {
				if (Answer == string.Empty) {
					MessageBox.Show($"答案{Number}為空。");
					return false;
				} else {
					int Count = Regex.Matches(Context, Regex.Escape(Answer)).Count;

					if (Count == 0) {
						MessageBox.Show($"答案{Number}錯誤。");
						return false;
					} else if (Count > 1) {
						if (AnswerNumber <= 0) {
							MessageBox.Show($"答案{Number}模糊。");
							return false;
						} else if (AnswerNumber > Count) {
							MessageBox.Show($"答案{Number}超出。");
							return false;
						}
						Answer += $" {AnswerNumber}";
					}
					Answers += Question + Environment.NewLine + Answer + Environment.NewLine;
					AnswerCount += 1;
				}
			}
			return true;
		}

		public static void MarkAnswer(FlowDocument ContextFlowDocument, string Context, string Answer, ref int AnswerNumber) {
			int AnswerNumberNow = AnswerNumber;
			ContextFlowDocument.Blocks.Clear();

			Context.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.None).ToList().ForEach(Line => {
				Paragraph ContextParagraph = new Paragraph();
				bool isAnswerInLine = Line.ToLower().Contains(Answer.Trim().ToLower());
				Regex.Split(Line, $"({Regex.Escape(Answer.Trim())})", RegexOptions.IgnoreCase).ToList().ForEach(Text => {
					Run Run = new Run(Text);
					ContextParagraph.Inlines.Add(Run);
					bool isAnswer = Text.ToLower() == Answer.Trim().ToLower();
					if (isAnswerInLine) Run.Foreground = AnswerForegroundBrush;
					if (isAnswerInLine && !isAnswer) Run.Background = HighLineBackgroundBrush;
					if (isAnswer) {
						Run.Background = AnswerBackgroundBrush;
						if (AnswerNumberNow > 0) {
							TextBlock Inline = new TextBlock() {
								Text = (++AnswerNumberNow).ToString(),
								Foreground = TaggedForegroundBrush,
								Background = InlineBackgroundBrush,
								FontSize = ContextFlowDocument.FontSize / 1.5
							};
							Inline.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
							Inline.Arrange(new Rect(Inline.DesiredSize));
							Inline.Margin = new Thickness(Inline.ActualWidth * -1D, 0, 0, 0);
							ContextParagraph.Inlines.Add(Inline);
						}


					}
				});
				ContextFlowDocument.Blocks.Add(ContextParagraph);
			});

			AnswerNumber = AnswerNumberNow;
		}

		public static void MarkAnswerWithRegex(FlowDocument document, string content, Regex[] regexs) {
			document.Blocks.Clear();

			content.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.None).ToList().ForEach(line => {
				Paragraph paragraph = new Paragraph();

				bool isAnswerInLine = false;
				bool[] charsIsMatched = new bool[line.Length];
				foreach (Regex regex in regexs) {
					MatchCollection matches = regex.Matches(line);
					foreach (Match match in matches)
						for (int i = match.Index; i < match.Index + match.Length; i++)
							isAnswerInLine = charsIsMatched[i] = true;
				}

				string text = string.Empty;
				for (int i = 0; i < line.Length; i++) {
					text += line[i];
					if (i < line.Length - 1 && charsIsMatched[i + 1] == charsIsMatched[i]) continue;
					Run run = new Run(text);
					paragraph.Inlines.Add(run);
					if (isAnswerInLine) run.Foreground = AnswerForegroundBrush;
					if (isAnswerInLine && !charsIsMatched[i]) run.Background = HighLineBackgroundBrush;
					if (charsIsMatched[i]) run.Background = AnswerBackgroundBrush;
					text = string.Empty;
				}
				document.Blocks.Add(paragraph);
			});
		}

		public static void UnmarkAnswer(FlowDocument ContextFlowDocument, string Context) {
			ContextFlowDocument.Blocks.Clear();
			Context.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList().ForEach(Line =>
				ContextFlowDocument.Blocks.Add(new Paragraph(new Run(Line)))
			);
		}
	}
}
