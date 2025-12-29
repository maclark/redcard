using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;


namespace RedCard {

    public enum BookElementType {
        Paragraph,
        Space
    }

    public class BookElement {
        public BookElementType Type;
        public string Text;      // for Paragraph
        public int SpaceLines;   // for Space
    }

    public class BookBlock {
        public int Column;
        public int Indent; // 0, 1, or 2
        public List<BookElement> Elements = new();
    }

    public class BookPage {
        public int PageNumber;
        public List<BookBlock> Blocks = new();
    }

    public class BookDocument {
        public List<BookPage> Pages = new();
    }


    public static class BookParser {
        private static readonly Regex ColumnRegex =
            new(@"\[column=(\d+)\s+indent=(\d+)\]", RegexOptions.IgnoreCase);

        private static readonly Regex SpaceRegex =
            new(@"\[space=(\d+)\]", RegexOptions.IgnoreCase);

        public static BookDocument Parse(string text) {
            var lines = File.ReadAllLines(text);

            var doc = new BookDocument();

            BookPage currentPage = null;
            BookBlock currentBlock = null;

            var paragraphBuffer = new StringBuilder();
            int blankLineCount = 0;

            void FlushParagraph() {
                if (paragraphBuffer.Length == 0 || currentBlock == null)
                    return;

                currentBlock.Elements.Add(new BookElement {
                    Type = BookElementType.Paragraph,
                    Text = paragraphBuffer.ToString().TrimEnd()
                });

                paragraphBuffer.Clear();
            }

            void FlushBlankLines() {
                if (blankLineCount == 0 || currentBlock == null)
                    return;

                currentBlock.Elements.Add(new BookElement {
                    Type = BookElementType.Space,
                    SpaceLines = blankLineCount
                });

                blankLineCount = 0;
            }

            foreach (var rawLine in lines) {
                var line = rawLine.TrimEnd();

                // ---------- Page ----------
                if (line.StartsWith("#page", StringComparison.OrdinalIgnoreCase)) {
                    FlushParagraph();
                    FlushBlankLines();

                    currentPage = new BookPage {
                        PageNumber = doc.Pages.Count
                    };

                    doc.Pages.Add(currentPage);
                    currentBlock = null;
                    continue;
                }

                // ---------- Column ----------
                var columnMatch = ColumnRegex.Match(line);
                if (columnMatch.Success) {
                    FlushParagraph();
                    FlushBlankLines();

                    if (currentPage == null)
                        throw new Exception("Column defined before page.");

                    currentBlock = new BookBlock {
                        Column = int.Parse(columnMatch.Groups[1].Value),
                        Indent = int.Parse(columnMatch.Groups[2].Value)
                    };

                    currentPage.Blocks.Add(currentBlock);
                    continue;
                }

                // ---------- Explicit space ----------
                var spaceMatch = SpaceRegex.Match(line);
                if (spaceMatch.Success) {
                    FlushParagraph();
                    FlushBlankLines();

                    currentBlock?.Elements.Add(new BookElement {
                        Type = BookElementType.Space,
                        SpaceLines = int.Parse(spaceMatch.Groups[1].Value)
                    });

                    continue;
                }

                // ---------- Blank line ----------
                if (string.IsNullOrWhiteSpace(line)) {
                    FlushParagraph();
                    blankLineCount++;
                    continue;
                }

                // ---------- Normal text ----------
                FlushBlankLines();

                if (paragraphBuffer.Length > 0)
                    paragraphBuffer.Append("\n");

                paragraphBuffer.Append(line);
            }

            // Final flush
            FlushParagraph();
            FlushBlankLines();

            return doc;
        }
    }
}
