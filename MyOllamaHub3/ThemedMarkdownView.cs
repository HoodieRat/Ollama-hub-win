using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace MyOllamaHub3
{
	public class ThemedMarkdownView : UserControl
	{
        private const int CornerRadius = 18;
		private const string CodeTokenPrefix = "__CODE_TOKEN_";

		private static readonly Regex InlineCodeRegex = new Regex(@"(`+)(.+?)\1", RegexOptions.Compiled | RegexOptions.Singleline);
		private static readonly Regex PipeRowRegex = new Regex(@"^\s*\|", RegexOptions.Compiled);
		private static readonly Regex PipeDividerCellRegex = new Regex(@"^:?[-=_\u2010-\u2015\u2500-\u257F]{3,}:?$", RegexOptions.Compiled);
		private static readonly Regex BulletRegex = new Regex(@"^\s*[-*]\s+", RegexOptions.Compiled);
		private static readonly Regex NumberedRegex = new Regex(@"^\s*\d+\.\s+", RegexOptions.Compiled);
		private static readonly Regex BlockquoteRegex = new Regex(@"^\s{0,3}>\s?", RegexOptions.Compiled);

		private readonly WebView2 _web;
		private bool _ready;
		private readonly StringBuilder _streamBuffer = new StringBuilder();
		private readonly StringBuilder _streamRawBuffer = new StringBuilder();

		private static readonly HashSet<string> AllowedHtmlTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"div","p","span","br","strong","em","b","i","code","pre","blockquote",
			"h1","h2","h3","h4","h5","h6","ul","ol","li","table","thead","tbody",
			"tr","th","td","a","img","hr","sup","sub"
		};

		private static readonly HashSet<string> SelfClosingHtmlTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"br","img","hr"
		};

		private static readonly Dictionary<string, HashSet<string>> AllowedHtmlAttributes = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
		{
			{ "a", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "href", "title", "target", "rel" } },
			{ "img", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "src", "alt", "title", "width", "height" } },
			{ "div", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } },
			{ "span", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } },
			{ "pre", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } },
			{ "code", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } },
			{ "table", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } },
			{ "th", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "colspan", "rowspan", "scope" } },
			{ "td", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "colspan", "rowspan", "scope" } }
		};

		private static readonly HashSet<string> EmptyAttributeSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private static readonly Regex HtmlTagRegex = new Regex(@"<(?<slash>/)?(?<name>[a-zA-Z0-9]+)(?<attrs>[^>]*)>", RegexOptions.Compiled);
		private static readonly Regex HtmlAttributeRegex = new Regex("(?<name>[a-zA-Z_:][\\w\\-.:]*)\\s*(=\\s*(?<value>\"[^\"]*\"|'[^']*'|[^>\\s\"']+))?", RegexOptions.Compiled);
		private static readonly Regex ScriptLikeRegex = new Regex(@"<(script|style)[^>]*?>.*?</\1>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
		private static readonly Regex HtmlCommentRegex = new Regex(@"<!--.*?-->", RegexOptions.Compiled | RegexOptions.Singleline);

		private readonly string WarmSand = "#C6B091";
		private readonly string SaddleTan = "#AA8B63";
		private readonly string Umber = "#34271C";
		private readonly string Cream = "#F0E4D2";
		private readonly string BorderUmber = "#786656";
		private readonly string CodeBg = "#2B2B2B";
		private readonly string CodeInk = "#EAEAEA";

		public ThemedMarkdownView()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
			BackColor = ColorTranslator.FromHtml("#C6B091");
			_web = new WebView2
			{
				Dock = DockStyle.Fill,
				DefaultBackgroundColor = Color.Transparent
			};

			Controls.Add(_web);
			InitializeAsync();
			UpdateClipRegion();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			UpdateClipRegion();
		}

		private void UpdateClipRegion()
		{
			var rect = ClientRectangle;
			if (rect.Width <= 0 || rect.Height <= 0)
			{
				Region = null;
				return;
			}

			using var path = BuildRoundedPath(rect, CornerRadius);
			var newRegion = new Region(path);
			var oldRegion = Region;
			Region = newRegion;
			oldRegion?.Dispose();
		}

		private static GraphicsPath BuildRoundedPath(Rectangle bounds, int radius)
		{
			var path = new GraphicsPath();
			var diameter = radius * 2;

			if (radius <= 0)
			{
				path.AddRectangle(bounds);
				path.CloseFigure();
				return path;
			}

			var arcRect = new Rectangle(bounds.Location, new Size(diameter, diameter));

			// Top left
			path.AddArc(arcRect, 180, 90);

			// Top right
			arcRect.X = bounds.Right - diameter;
			path.AddArc(arcRect, 270, 90);

			// Bottom right
			arcRect.Y = bounds.Bottom - diameter;
			path.AddArc(arcRect, 0, 90);

			// Bottom left
			arcRect.X = bounds.Left;
			path.AddArc(arcRect, 90, 90);

			path.CloseFigure();
			return path;
		}

		private async void InitializeAsync()
		{
			try
			{
				var env = await CoreWebView2Environment.CreateAsync();
				await _web.EnsureCoreWebView2Async(env);
				_ready = true;

				_web.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
				_web.CoreWebView2.Settings.IsStatusBarEnabled = false;
				_web.CoreWebView2.Settings.AreDevToolsEnabled = false;

				_web.CoreWebView2.NewWindowRequested += (s, e) =>
				{
					e.Handled = true;
					if (!string.IsNullOrEmpty(e.Uri))
						BrowserLauncher.TryOpenUrl(e.Uri, out _);
				};

				_web.CoreWebView2.NavigationStarting += (s, e) =>
				{
					if (string.IsNullOrEmpty(e.Uri))
						return;

					if (e.Uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
						e.Uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
					{
						e.Cancel = true;
						BrowserLauncher.TryOpenUrl(e.Uri, out _);
					}
				};

				_web.CoreWebView2.NavigateToString(BuildChatShellHtml());
			}
			catch (Exception ex)
			{
				MessageBox.Show($"WebView2 init failed: {ex.Message}", "WebView2", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void Clear()
		{
			_streamBuffer.Clear();
			if (_ready)
				_web.CoreWebView2.NavigateToString(BuildChatShellHtml());
		}

		public void LoadSmart(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				Clear();
				return;
			}

			string trimmed = input.Trim();
			if (LooksLikeHtml(trimmed))
			{
				LoadHtml(trimmed);
				return;
			}

			if (LooksLikeMarkdown(trimmed))
			{
				LoadMarkdown(trimmed);
				return;
			}

			LoadMarkdown(SmartConvertToMarkdown(trimmed));
		}

		public void LoadMarkdown(string md)
		{
			_streamBuffer.Clear();
			var body = MarkdownToHtml(md ?? string.Empty);
			LoadHtml(WrapContentPage(body));
		}

		public void LoadHtml(string bodyInnerHtml)
		{
			_streamBuffer.Clear();
			var safeBody = SanitizeHtml(bodyInnerHtml ?? string.Empty);
			if (!_ready)
			{
				_web.CoreWebView2InitializationCompleted += (s, e) =>
					_web.CoreWebView2.NavigateToString(WrapContentPage(safeBody));
				return;
			}

			_web.CoreWebView2.NavigateToString(WrapContentPage(safeBody));
		}

		public void AddUserMessage(string content)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(AddUserMessage), content);
				return;
			}

			var html = MarkdownToHtml(content ?? string.Empty);
			Exec("appendBlock('user', " + JsLiteral(html) + ");");
		}

		public void AddAssistantMessage(string content)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(AddAssistantMessage), content);
				return;
			}

			var sanitized = StreamingUpdateHelper.StripHiddenSections(content);
			var html = MarkdownToHtml(sanitized);
			Exec("appendAssistantBlock(" + JsLiteral(html) + ");");
			_streamBuffer.Clear();
			_streamBuffer.Append(sanitized);
			_streamRawBuffer.Clear();
			_streamRawBuffer.Append(sanitized);
		}

		public void AddSystemMessage(string content)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(AddSystemMessage), content);
				return;
			}

			var html = MarkdownToHtml(content ?? string.Empty);
			Exec("appendSystemBlock(" + JsLiteral(html) + ");");
		}

		public void AppendTextSafe(string delta) => AppendAssistantDelta(delta);

		public void AppendAssistantDelta(string delta)
		{
			if (string.IsNullOrEmpty(delta))
				return;

			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(AppendAssistantDelta), delta);
				return;
			}

			_streamRawBuffer.Append(delta);
			var sanitized = StreamingUpdateHelper.StripHiddenSections(_streamRawBuffer.ToString());
			_streamBuffer.Clear();
			_streamBuffer.Append(sanitized);
			var html = MarkdownToHtml(_streamBuffer.ToString(), allowTrailingPipeTables: false);
			Exec("renderAssistant(" + JsLiteral(html) + ");");
		}

		public void ReplaceAssistantMessage(string content)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(ReplaceAssistantMessage), content);
				return;
			}

			var sanitized = StreamingUpdateHelper.StripHiddenSections(content);
			_streamRawBuffer.Clear();
			_streamRawBuffer.Append(sanitized);
			_streamBuffer.Clear();
			_streamBuffer.Append(sanitized);
			var html = MarkdownToHtml(sanitized);
			Exec("renderAssistant(" + JsLiteral(html) + ");");
		}

		public void EndAssistantMessage()
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action(EndAssistantMessage));
				return;
			}

			if (_streamRawBuffer.Length > 0)
			{
				var sanitized = StreamingUpdateHelper.StripHiddenSections(_streamRawBuffer.ToString());
				_streamBuffer.Clear();
				_streamBuffer.Append(sanitized);
				var html = MarkdownToHtml(_streamBuffer.ToString());
				Exec("renderAssistant(" + JsLiteral(html) + ");");
				_streamRawBuffer.Clear();
				_streamBuffer.Clear();
			}

			Exec("completeAssistantMessage();");
		}

		private void Exec(string js)
		{
			if (!_ready)
				return;

			_web.CoreWebView2.ExecuteScriptAsync(js);
		}

		private string BuildChatShellHtml()
		{
			var sb = new StringBuilder();
			sb.Append("<!DOCTYPE html><html lang='en'><head><meta charset='utf-8'>");
			sb.Append("<meta name='viewport' content='width=device-width, initial-scale=1'>");
			sb.Append("<title>Chat</title><style>");
			sb.Append(":root{");
			sb.Append("--bg:").Append(WarmSand).Append(";--ink:").Append(Umber).Append(";");
			sb.Append("--cream:").Append(Cream).Append(";--tan:").Append(SaddleTan).Append(";");
			sb.Append("--border:").Append(BorderUmber).Append(";--codebg:").Append(CodeBg).Append(";");
			sb.Append("--codeink:").Append(CodeInk).Append(";");
			sb.Append("}");
			sb.Append("html,body{height:100%;margin:0;}body{background:var(--bg);color:var(--ink);font-family:'Segoe UI',Inter,system-ui,-apple-system,Arial,sans-serif;line-height:1.45;padding:18px 20px 22px;box-sizing:border-box;display:flex;}");
			sb.Append("#canvas{flex:1;display:flex;height:100%;border-radius:22px;border:1px solid rgba(90,66,40,0.55);background:linear-gradient(165deg,rgba(255,244,226,0.82),rgba(219,186,142,0.38));box-shadow:0 18px 40px rgba(0,0,0,0.28),inset 0 1px 0 rgba(255,255,255,0.45);overflow:hidden;backdrop-filter:blur(4px);}");
			sb.Append("#log{flex:1;overflow:auto;padding:46px 30px 34px 30px;box-sizing:border-box;background:linear-gradient(175deg,rgba(255,250,243,0.88),rgba(237,207,167,0.24));}");
			sb.Append(".msg{max-width:100%;margin:12px 0;display:flex;}");
			sb.Append(".msg:first-child{margin-top:0;}");
			sb.Append(".msg .bubble{border-radius:16px;padding:12px 14px;border:1px solid var(--border);box-sizing:border-box;word-wrap:break-word;overflow-wrap:anywhere;}");
			sb.Append(".msg.user{justify-content:flex-end;}.msg.user .bubble{background:#efe6d7;border-color:var(--border);}");
			sb.Append(".msg.assistant{justify-content:flex-start;}.msg.assistant .bubble{background:var(--tan);color:var(--cream);border-color:var(--border);}");
			sb.Append(".msg.system{justify-content:center;}.msg.system .bubble{background:#e4d9c6;color:var(--ink);border-style:dashed;}");
			sb.Append(".bubble .table-scroll{max-width:100%;overflow:hidden;margin:10px 0;border-radius:10px;border:1px solid rgba(255,218,170,0.22);background:linear-gradient(145deg,rgba(12,12,12,0.95),rgba(8,8,10,0.98));position:relative;box-shadow:0 12px 28px rgba(0,0,0,0.45),inset 0 1px 0 rgba(255,255,255,0.05);backdrop-filter:blur(2px);opacity:0;transform:translateY(6px);animation:tableReveal .24s ease-out forwards;}");
			sb.Append(".bubble .table-scroll::after{content:'';position:absolute;top:0;left:0;right:0;height:3px;background:linear-gradient(90deg,rgba(255,200,140,0.45),rgba(255,255,255,0));opacity:.8;}");
			sb.Append(".bubble table{border-collapse:separate;border-spacing:0;width:100%;background:rgba(14,14,16,0.96);color:#f8f0e4;table-layout:fixed;margin:0;}");
			sb.Append(".bubble table thead tr{background:linear-gradient(180deg,rgba(72,52,28,0.95),rgba(28,20,12,0.95));}");
			sb.Append(".bubble th,.bubble td{border:1px solid rgba(255,214,170,0.16);padding:8px 12px;word-break:break-word;overflow-wrap:anywhere;white-space:normal;vertical-align:top;}");
			sb.Append(".bubble th{font-weight:600;color:#fff1e4;text-shadow:0 1px 2px rgba(0,0,0,0.45);}");
			sb.Append(".bubble tbody tr:nth-child(even){background:rgba(30,24,20,0.65);}");
			sb.Append(".bubble tbody tr:hover{background:rgba(68,48,26,0.55);}");
			sb.Append(".bubble table tr:first-child th:first-child{border-top-left-radius:10px;}");
			sb.Append(".bubble table tr:first-child th:last-child{border-top-right-radius:10px;}");
			sb.Append(".bubble table tr:last-child td:first-child{border-bottom-left-radius:10px;}");
			sb.Append(".bubble table tr:last-child td:last-child{border-bottom-right-radius:10px;}");
			sb.Append(".bubble pre{background:var(--codebg);color:var(--codeink);padding:10px;border-radius:8px;overflow:auto;box-shadow:inset 0 0 0 1px rgba(255,255,255,0.04);}");
			sb.Append(".bubble pre.pending-table{background:rgba(18,16,14,0.85);color:#f3e6d6;border:1px solid rgba(255,214,170,0.18);box-shadow:0 6px 16px rgba(0,0,0,0.35),inset 0 0 0 1px rgba(255,255,255,0.03);}");
			sb.Append("@keyframes tableReveal{0%{opacity:0;transform:translateY(6px);}100%{opacity:1;transform:translateY(0);}}");
			sb.Append(".bubble code{font-family:'Cascadia Code',Consolas,monospace;white-space:pre-wrap;word-break:break-word;overflow-wrap:anywhere;}");
			sb.Append(".bubble a{color:#4060a0;text-decoration:none;}.bubble a:hover{text-decoration:underline;}");
			sb.Append("</style></head><body><div id='canvas'><div id='log'></div></div><script>");
			sb.Append("function ensureStreamingAssistant(forceNew){const log=document.getElementById('log');let last=log.lastElementChild;");
			sb.Append("if(forceNew&&last&&last.classList.contains('assistant')){last.dataset.streaming='false';last=null;}");
			sb.Append("if(!last||!last.classList.contains('assistant')||last.dataset.streaming!=='true'){if(last&&last.classList.contains('assistant')){last.dataset.streaming='false';}");
			sb.Append("last=document.createElement('div');last.className='msg assistant';last.dataset.streaming='true';const bub=document.createElement('div');bub.className='bubble';last.appendChild(bub);log.appendChild(last);}return last.querySelector('.bubble');}");
			sb.Append("function appendBlock(role,htmlBlock){const log=document.getElementById('log');const msg=document.createElement('div');msg.className='msg '+role;const bub=document.createElement('div');bub.className='bubble';bub.innerHTML=htmlBlock;msg.appendChild(bub);log.appendChild(msg);log.scrollTop=log.scrollHeight;}");
			sb.Append("function renderAssistant(htmlBlock){const bubble=ensureStreamingAssistant(false);bubble.innerHTML=htmlBlock;bubble.parentElement.dataset.streaming='true';const log=document.getElementById('log');log.scrollTop=log.scrollHeight;}");
			sb.Append("function appendAssistantBlock(htmlBlock){const log=document.getElementById('log');const msg=document.createElement('div');msg.className='msg assistant';msg.dataset.streaming='false';const bubble=document.createElement('div');bubble.className='bubble';bubble.innerHTML=htmlBlock;msg.appendChild(bubble);log.appendChild(msg);log.scrollTop=log.scrollHeight;}");
			sb.Append("function appendSystemBlock(htmlBlock){const log=document.getElementById('log');const msg=document.createElement('div');msg.className='msg system';const bubble=document.createElement('div');bubble.className='bubble';bubble.innerHTML=htmlBlock;msg.appendChild(bubble);log.appendChild(msg);log.scrollTop=log.scrollHeight;}");
			sb.Append("function completeAssistantMessage(){const log=document.getElementById('log');const last=log.lastElementChild;if(last&&last.classList.contains('assistant')){last.dataset.streaming='false';}}");
			sb.Append("</script></body></html>");
			return sb.ToString();
		}

		private string WrapContentPage(string inner)
		{
			var sb = new StringBuilder();
			sb.Append("<!DOCTYPE html><html lang='en'><head><meta charset='UTF-8'>");
			sb.Append("<meta name='viewport' content='width=device-width, initial-scale=1.0'><title>Output</title><style>");
			sb.Append(":root{--bg:").Append(WarmSand).Append(";--ink:").Append(Umber).Append(";--tan:")
			  .Append(SaddleTan).Append(";--border:").Append(BorderUmber).Append(";--codebg:")
			  .Append(CodeBg).Append(";--codeink:").Append(CodeInk).Append(";}");
			sb.Append("html,body{height:100%;margin:0;}body{background:var(--bg);color:var(--ink);font-family:'Segoe UI',Inter,sans-serif;line-height:1.45;}");
			sb.Append(".md{padding:10px 14px;}a{color:#4060a0;text-decoration:none;}a:hover{text-decoration:underline;}");
			sb.Append("pre.code{background:var(--codebg);color:var(--codeink);padding:10px;border-radius:6px;overflow:auto;}pre.code.pending-table{background:rgba(18,16,14,0.85);color:#f3e6d6;border:1px solid rgba(255,214,170,0.18);box-shadow:0 6px 16px rgba(0,0,0,0.35),inset 0 0 0 1px rgba(255,255,255,0.03);}code.inline{background:rgba(0,0,0,.07);padding:1px 4px;border-radius:3px;white-space:pre-wrap;word-break:break-word;overflow-wrap:anywhere;}");
			sb.Append(".table-scroll{max-width:100%;overflow:hidden;margin:10px 0;border-radius:10px;border:1px solid rgba(255,218,170,0.22);background:linear-gradient(145deg,rgba(12,12,12,0.95),rgba(8,8,10,0.98));position:relative;box-shadow:0 12px 28px rgba(0,0,0,0.45),inset 0 1px 0 rgba(255,255,255,0.05);backdrop-filter:blur(2px);opacity:0;transform:translateY(6px);animation:tableReveal .24s ease-out forwards;}");
			sb.Append(".table-scroll::after{content:'';position:absolute;top:0;left:0;right:0;height:3px;background:linear-gradient(90deg,rgba(255,200,140,0.45),rgba(255,255,255,0));opacity:.75;}");
			sb.Append("table{border-collapse:separate;border-spacing:0;width:100%;background:rgba(14,14,16,0.96);margin:0;table-layout:fixed;color:#f8f0e4;}");
			sb.Append("table thead tr{background:linear-gradient(180deg,rgba(72,52,28,0.95),rgba(28,20,12,0.95));}");
			sb.Append("th,td{border:1px solid rgba(255,214,170,0.16);padding:8px 12px;word-break:break-word;overflow-wrap:anywhere;white-space:normal;vertical-align:top;}");
			sb.Append("th{font-weight:600;color:#fff1e4;text-shadow:0 1px 2px rgba(0,0,0,0.45);}");
			sb.Append("tbody tr:nth-child(even){background:rgba(30,24,20,0.65);}");
			sb.Append("tbody tr:hover{background:rgba(68,48,26,0.55);}");
			sb.Append("table tr:first-child th:first-child{border-top-left-radius:10px;}");
			sb.Append("table tr:first-child th:last-child{border-top-right-radius:10px;}");
			sb.Append("table tr:last-child td:first-child{border-bottom-left-radius:10px;}");
			sb.Append("table tr:last-child td:last-child{border-bottom-right-radius:10px;}");
			sb.Append("@keyframes tableReveal{0%{opacity:0;transform:translateY(6px);}100%{opacity:1;transform:translateY(0);}}");
			sb.Append("</style></head><body>").Append(inner).Append("</body></html>");
			return sb.ToString();
		}

		private static bool LooksLikeHtml(string s) =>
			s.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
			Regex.IsMatch(s, @"<html\b|<head\b|<body\b", RegexOptions.IgnoreCase) ||
			Regex.IsMatch(s, @"</?(table|div|span|p|a|pre|code|ul|ol|li|h\d)\b", RegexOptions.IgnoreCase);

		private static bool LooksLikeMarkdown(string s) =>
			s.Contains("```") ||
			Regex.IsMatch(s, @"(^|\n)#{1,6}\s+\S") ||
			Regex.IsMatch(s, @"(^|\n)\s*[-*+]\s+\S") ||
			Regex.IsMatch(s, @"(^|\n)\|.+\|\s*\n\|?\s*:?-{3,}") ||
			Regex.IsMatch(s, @"\[(.*?)\]\((https?://[^\s)]+)\)");

		private string SmartConvertToMarkdown(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return string.Empty;

			var blocks = Regex.Split(text.Replace("\r\n", "\n"), @"\n\s*\n")
							  .Select(b => b.Trim('\n'))
							  .Where(b => !string.IsNullOrWhiteSpace(b))
							  .ToList();

			var sb = new StringBuilder();

			foreach (var block in blocks)
			{
				if (TryConvertCsv(block, out var csvMd))
				{
					sb.AppendLine(csvMd);
				}
				else if (TryConvertTsv(block, out var tsvMd))
				{
					sb.AppendLine(tsvMd);
				}
				else if (TryConvertSpaceColumns(block, out var spaceMd))
				{
					sb.AppendLine(spaceMd);
				}
				else if (TryConvertKeyValue(block, out var kvMd))
				{
					sb.AppendLine(kvMd);
				}
				else
				{
					sb.AppendLine(AutoLink(block));
				}

				sb.AppendLine();
			}

			return sb.ToString().Trim();
		}

		private static string AutoLink(string s) =>
			Regex.Replace(s, @"(?<!\()(?<url>https?://[^\s)]+)",
				m => "[" + m.Groups["url"].Value + "](" + m.Groups["url"].Value + ")");

		private static bool TryConvertCsv(string block, out string md)
		{
			md = string.Empty;
			var lines = block.Split('\n');
			if (lines.Length < 2 || !lines.Any(l => l.Contains(',')))
				return false;

			var rows = new List<List<string>>();
			foreach (var line in lines)
			{
				var row = ParseDelimited(line, ',');
				if (row.Count <= 1)
					return false;
				rows.Add(row);
			}

			md = RowsToMarkdownTable(rows);
			return true;
		}

		private static bool TryConvertTsv(string block, out string md)
		{
			md = string.Empty;
			var lines = block.Split('\n');
			if (lines.Length < 2 || !lines.Any(l => l.Contains('\t')))
				return false;

			var rows = lines.Select(l => l.Split('\t').Select(x => x.Trim()).ToList()).ToList();
			if (rows.All(r => r.Count == rows[0].Count && r.Count > 1))
			{
				md = RowsToMarkdownTable(rows);
				return true;
			}

			return false;
		}

		private static bool TryConvertSpaceColumns(string block, out string md)
		{
			md = string.Empty;
			var lines = block.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
			if (lines.Count < 2)
				return false;

			var rows = new List<List<string>>();
			foreach (var line in lines)
			{
				var parts = Regex.Split(line.Trim(), @"\s{2,}").Where(x => x.Length > 0).ToList();
				if (parts.Count <= 1)
				{
					rows.Clear();
					break;
				}
				rows.Add(parts);
			}

			if (rows.Count >= 2 && rows.All(r => r.Count == rows[0].Count))
			{
				md = RowsToMarkdownTable(rows);
				return true;
			}

			return false;
		}

		private static bool TryConvertKeyValue(string block, out string md)
		{
			md = string.Empty;
			var lines = block.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
			if (lines.Count < 2)
				return false;

			var rows = new List<List<string>>();
			foreach (var l in lines)
			{
				var m = Regex.Match(l, @"^\s*(.+?)\s*[:=]\s*(.+?)\s*$");
				if (!m.Success)
				{
					rows.Clear();
					break;
				}
				rows.Add(new List<string> { m.Groups[1].Value.Trim(), m.Groups[2].Value.Trim() });
			}

			if (rows.Count >= 2)
			{
				rows.Insert(0, new List<string> { "Field", "Value" });
				md = RowsToMarkdownTable(rows);
				return true;
			}

			return false;
		}

		private static string RowsToMarkdownTable(List<List<string>> rows)
		{
			if (rows == null || rows.Count == 0)
				return string.Empty;

			var sb = new StringBuilder();
			var header = rows[0];
			sb.Append("| ").Append(string.Join(" | ", header.Select(EscapeMdCell))).AppendLine(" |");
			sb.Append("| ").Append(string.Join(" | ", header.Select(_ => "---"))).AppendLine(" |");
			foreach (var row in rows.Skip(1))
				sb.Append("| ").Append(string.Join(" | ", row.Select(EscapeMdCell))).AppendLine(" |");
			return sb.ToString().TrimEnd();
		}

		private static string EscapeMdCell(string s) => (s ?? string.Empty).Replace("|", "\\|");

		private static List<string> ParseDelimited(string line, char delim)
		{
			var result = new List<string>();
			if (line == null)
				return result;

			var sb = new StringBuilder();
			bool inQuotes = false;
			for (int i = 0; i < line.Length; i++)
			{
				char c = line[i];
				if (inQuotes)
				{
					if (c == '"')
					{
						if (i + 1 < line.Length && line[i + 1] == '"')
						{
							sb.Append('"');
							i++;
						}
						else
						{
							inQuotes = false;
						}
					}
					else
					{
						sb.Append(c);
					}
				}
				else
				{
					if (c == '"')
					{
						inQuotes = true;
					}
					else if (c == delim)
					{
						result.Add(sb.ToString().Trim());
						sb.Clear();
					}
					else
					{
						sb.Append(c);
					}
				}
			}

			result.Add(sb.ToString().Trim());
			return result;
		}

		private string MarkdownToHtml(string md, bool allowTrailingPipeTables = true)
		{
			var segments = new List<HtmlSegment>();
			var tableSlots = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			var lines = (md ?? string.Empty).Replace("\r\n", "\n").Split('\n');
			bool inCode = false;
			var codeBlockLines = new List<string>();
			var renderedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			segments.Add(new HtmlSegment("<div class=\"md\">", null));
			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];

				if (line.StartsWith("```") )
				{
					if (!inCode)
					{
						inCode = true;
						codeBlockLines.Clear();
					}
					else
					{
						inCode = false;
						if (!TryEmitTableFromLines(codeBlockLines, renderedTables, segments, tableSlots))
						{
							segments.Add(new HtmlSegment("<pre class=\"code\"><code>" + string.Join("\n", codeBlockLines.Select(HtmlEscape)) + "</code></pre>", null));
						}
						codeBlockLines.Clear();
					}
					continue;
				}

				if (inCode)
				{
					codeBlockLines.Add(line);
					continue;
				}

				if (PipeRowRegex.IsMatch(line))
				{
					var block = new List<string>();
					int j = i;
					while (j < lines.Length && PipeRowRegex.IsMatch(lines[j]))
					{
						block.Add(lines[j]);
						j++;
					}

					if (!allowTrailingPipeTables)
					{
						segments.Add(new HtmlSegment(BuildPendingTablePreview(block), null));
						i = j - 1;
						continue;
					}

					var tableResult = TryRenderPipeTable(block, renderedTables);
					if (tableResult.HasValue)
						AddOrReplaceTableSegment(segments, tableSlots, tableResult.Value);

					i = j - 1;
					continue;
				}

				if (line.StartsWith("#"))
				{
					int level = Math.Min(6, line.TakeWhile(ch => ch == '#').Count());
					string text = line.Substring(level).Trim();
					segments.Add(new HtmlSegment("<h" + level + ">" + InlineFormat(text) + "</h" + level + ">", null));
					continue;
				}

				if (BulletRegex.IsMatch(line))
				{
					var listBuilder = new StringBuilder();
					listBuilder.Append("<ul>");
					while (i < lines.Length && BulletRegex.IsMatch(lines[i]))
					{
						var item = BulletRegex.Replace(lines[i], string.Empty);
						listBuilder.Append("<li>").Append(InlineFormat(item)).Append("</li>");
						i++;
					}
					listBuilder.Append("</ul>");
					segments.Add(new HtmlSegment(listBuilder.ToString(), null));
					i--;
					continue;
				}

				if (NumberedRegex.IsMatch(line))
				{
					var listBuilder = new StringBuilder();
					listBuilder.Append("<ol>");
					while (i < lines.Length && NumberedRegex.IsMatch(lines[i]))
					{
						var item = NumberedRegex.Replace(lines[i], string.Empty);
						listBuilder.Append("<li>").Append(InlineFormat(item)).Append("</li>");
						i++;
					}
					listBuilder.Append("</ol>");
					segments.Add(new HtmlSegment(listBuilder.ToString(), null));
					i--;
					continue;
				}

				if (BlockquoteRegex.IsMatch(line))
				{
					var quoteLines = new List<string>();
					int j = i;
					while (j < lines.Length && BlockquoteRegex.IsMatch(lines[j]))
					{
						var content = BlockquoteRegex.Replace(lines[j], string.Empty);
						quoteLines.Add(content);
						j++;
					}

					var quoteBuilder = new StringBuilder();
					quoteBuilder.Append("<blockquote>");
					foreach (var qLine in quoteLines)
					{
						if (string.IsNullOrWhiteSpace(qLine))
						{
							quoteBuilder.Append("<div class=\"sp\"></div>");
						}
						else
						{
							quoteBuilder.Append("<p>").Append(InlineFormat(qLine)).Append("</p>");
						}
					}
					quoteBuilder.Append("</blockquote>");
					segments.Add(new HtmlSegment(quoteBuilder.ToString(), null));
					i = j - 1;
					continue;
				}

				if (string.IsNullOrWhiteSpace(line))
				{
					segments.Add(new HtmlSegment("<div class=\"sp\"></div>", null));
				}
				else
				{
					segments.Add(new HtmlSegment("<p>" + InlineFormat(line) + "</p>", null));
				}
			}

			if (inCode && codeBlockLines.Count > 0)
			{
				if (!TryEmitTableFromLines(codeBlockLines, renderedTables, segments, tableSlots))
				{
					segments.Add(new HtmlSegment("<pre class=\"code\"><code>" + string.Join("\n", codeBlockLines.Select(HtmlEscape)) + "</code></pre>", null));
				}
			}

			segments.Add(new HtmlSegment("</div>", null));

			var output = new StringBuilder();
			foreach (var segment in segments)
				output.Append(segment.Html);
			return output.ToString();
		}

		private bool TryEmitTableFromLines(List<string> lines, HashSet<string> renderedTables, List<HtmlSegment> segments, Dictionary<string, int> tableSlots)
		{
			if (lines.Count == 0)
				return false;

			if (!lines.All(line => PipeRowRegex.IsMatch(line)))
				return false;

			var tableResult = TryRenderPipeTable(lines, renderedTables);
			if (!tableResult.HasValue)
				return false;

			AddOrReplaceTableSegment(segments, tableSlots, tableResult.Value);
			return true;
		}

		private TableRenderResult? TryRenderPipeTable(List<string> rawLines, HashSet<string> renderedTables)
		{
			if (!TryNormalizePipeTable(rawLines, out var header, out var bodyRows))
				return null;

			var signature = BuildTableSignature(header, bodyRows);
			if (renderedTables.Contains(signature))
				return null;

			renderedTables.Add(signature);
			var html = BuildTableHtml(header, bodyRows);
			var displayKey = BuildTableDisplayKey(header, bodyRows);
			return new TableRenderResult(html, displayKey);
		}

		private bool TryNormalizePipeTable(IEnumerable<string> rawLines, out List<string> header, out List<List<string>> bodyRows)
		{
			header = new List<string>();
			bodyRows = new List<List<string>>();

			if (rawLines == null)
				return false;

			var sanitizedLines = rawLines
				.Where(l => !string.IsNullOrWhiteSpace(l))
				.Select(l => l.Trim())
				.ToList();

			if (sanitizedLines.Count < 2)
				return false;

			var rows = new List<List<string>>();
			foreach (var line in sanitizedLines)
			{
				if (!line.Contains('|'))
					return false;

				var trimmed = line;
				if (!trimmed.StartsWith("|"))
					trimmed = "|" + trimmed;
				if (!trimmed.EndsWith("|"))
					trimmed += "|";

				var segments = trimmed.Split('|');
				if (segments.Length < 3)
					return false;

				var cells = segments
					.Skip(1)
					.Take(segments.Length - 2)
					.Select(c => c.Trim())
					.ToList();

				rows.Add(cells);
			}

			rows = StripNoiseRows(rows);

			if (rows.Count < 2)
				return false;

			var dividerIndex = rows.FindIndex(1, r => r.Count > 0 && r.All(cell => PipeDividerCellRegex.IsMatch(cell)));
			if (dividerIndex < 1)
				return false;

			header = rows[0];
			var bodyCandidate = rows.Skip(dividerIndex + 1)
				.Where(r => r.Any(cell => !PipeDividerCellRegex.IsMatch(cell)))
				.ToList();

			if (bodyCandidate.Count == 0)
				return false;

			int columnCount = Math.Max(header.Count, bodyCandidate.Max(r => r.Count));
			header = NormalizeRow(header, columnCount);
			bodyRows = bodyCandidate
				.Select(r => NormalizeRow(r, columnCount))
				.Where(RowHasContent)
				.ToList();

			if (!RowHasContent(header) && bodyRows.Count == 0)
				return false;

			if (bodyRows.Count == 0)
				return false;

			return true;
		}

		private static List<List<string>> StripNoiseRows(List<List<string>> rows)
		{
			if (rows == null || rows.Count == 0)
				return new List<List<string>>();

			var cleaned = new List<List<string>>();
			foreach (var row in rows)
			{
				if (row == null)
					continue;

				bool isEmpty = row.All(cell => string.IsNullOrWhiteSpace(cell));
				bool isBorder = row.All(IsBorderCell);

				if ((isEmpty || isBorder) && cleaned.Count == 0)
					continue;

				cleaned.Add(row);
			}

			while (cleaned.Count > 0)
			{
				var last = cleaned[cleaned.Count - 1];
				if (last.All(cell => string.IsNullOrWhiteSpace(cell)) || last.All(IsBorderCell))
					cleaned.RemoveAt(cleaned.Count - 1);
				else
					break;
			}

			return cleaned;
		}

		private static bool IsBorderCell(string cell)
		{
			if (string.IsNullOrWhiteSpace(cell))
				return true;

			foreach (char ch in cell)
			{
				if (!(ch == ':' || ch == '-' || ch == '=' || ch == '_' || ch == ' ' || (ch >= '\u2010' && ch <= '\u2015') || (ch >= '\u2500' && ch <= '\u257F')))
					return false;
			}

			return true;
		}

		private static string BuildTableSignature(List<string> header, List<List<string>> bodyRows)
		{
			string NormalizeCell(string cell) => Regex.Replace((cell ?? string.Empty).ToLowerInvariant(), @"\s+", " ").Trim();

			var sb = new StringBuilder();
			sb.Append(string.Join("|", header.Select(NormalizeCell)));
			foreach (var row in bodyRows)
				sb.Append('\n').Append(string.Join("|", row.Select(NormalizeCell)));
			return sb.ToString();
		}

		private string BuildTableHtml(List<string> header, List<List<string>> bodyRows)
		{
			var sb = new StringBuilder();
			sb.Append("<div class=\"table-scroll\"><table><thead><tr>");
			foreach (var cell in header)
				sb.Append("<th>").Append(InlineFormat(cell)).Append("</th>");
			sb.Append("</tr></thead><tbody>");
			foreach (var row in bodyRows)
			{
				sb.Append("<tr>");
				foreach (var cell in row)
					sb.Append("<td>").Append(InlineFormat(cell)).Append("</td>");
				sb.Append("</tr>");
			}
			sb.Append("</tbody></table></div>");
			return sb.ToString();
		}

		private string BuildPendingTablePreview(List<string> rawLines)
		{
			if (rawLines == null || rawLines.Count == 0)
				return "<pre class=\"code pending-table\"></pre>";

			var sb = new StringBuilder();
			sb.Append("<pre class=\"code pending-table\">");
			for (int index = 0; index < rawLines.Count; index++)
			{
				if (index > 0)
					sb.Append('\n');
				sb.Append(HtmlEscape(rawLines[index]));
			}
			sb.Append("</pre>");
			return sb.ToString();
		}

		private void AddOrReplaceTableSegment(List<HtmlSegment> segments, Dictionary<string, int> tableSlots, TableRenderResult table)
		{
			if (tableSlots.TryGetValue(table.DisplayKey, out int index))
			{
				segments[index] = new HtmlSegment(table.Html, table.DisplayKey);
			}
			else
			{
				tableSlots[table.DisplayKey] = segments.Count;
				segments.Add(new HtmlSegment(table.Html, table.DisplayKey));
			}
		}

		private static string BuildTableDisplayKey(List<string> header, List<List<string>> bodyRows)
		{
			var headerKey = BuildTableHeaderKey(header);
			var firstRowKey = bodyRows.Count > 0 ? string.Join("|", bodyRows[0].Select(NormalizeCellForKey)) : string.Empty;
			return headerKey + "#" + header.Count + "#" + firstRowKey;
		}

		private static string BuildTableHeaderKey(IEnumerable<string> header)
		{
			return string.Join("|", header.Select(NormalizeCellForKey));
		}

		private static string NormalizeCellForKey(string value)
		{
			return Regex.Replace((value ?? string.Empty).ToLowerInvariant(), @"\s+", " ").Trim();
		}

		private readonly struct HtmlSegment
		{
			public HtmlSegment(string html, string? tableKey)
			{
				Html = html;
				TableKey = tableKey;
			}

			public string Html { get; }
			public string? TableKey { get; }
		}

		private readonly struct TableRenderResult
		{
			public TableRenderResult(string html, string displayKey)
			{
				Html = html;
				DisplayKey = displayKey;
			}

			public string Html { get; }
			public string DisplayKey { get; }
		}


		private static List<string> NormalizeRow(List<string> row, int columnCount)
		{
			var normalized = new List<string>(row.Take(columnCount));
			while (normalized.Count < columnCount)
				normalized.Add(string.Empty);
			return normalized;
		}

		private static bool RowHasContent(IEnumerable<string> row) =>
			row.Any(cell => cell.Any(ch => char.IsLetterOrDigit(ch)));

		private string InlineFormat(string text)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;

			var codeTokens = new List<string>();
			var tokenized = InlineCodeRegex.Replace(text, m =>
			{
				var token = CodeTokenPrefix + codeTokens.Count + "__";
				var codeContent = RemoveOuterWhitespace(m.Groups[2].Value);
				codeTokens.Add("<code class=\"inline\">" + HtmlEscape(codeContent) + "</code>");
				return token;
			});

			var escaped = HtmlEscape(tokenized);

			escaped = Regex.Replace(escaped, @"\[(.+?)\]\((https?://[^\s)]+)\)", m =>
			{
				var href = m.Groups[2].Value;
				return "<a href=\"" + HtmlAttributeEscape(href) + "\" target=\"_blank\" rel=\"noopener\">" + m.Groups[1].Value + "</a>";
			}, RegexOptions.Compiled);

			escaped = Regex.Replace(escaped, @"\*\*(.+?)\*\*", "<strong>$1</strong>", RegexOptions.Compiled);
			escaped = Regex.Replace(escaped, @"__(.+?)__", "<strong>$1</strong>", RegexOptions.Compiled);
			escaped = Regex.Replace(escaped, @"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)", "<em>$1</em>", RegexOptions.Compiled);
			escaped = Regex.Replace(escaped, @"(?<!_)_(?!_)(.+?)(?<!_)_(?!_)", "<em>$1</em>", RegexOptions.Compiled);
			escaped = Regex.Replace(escaped, @"~~(.+?)~~", "<span style=\"text-decoration:line-through;\">$1</span>", RegexOptions.Compiled);

			for (int i = 0; i < codeTokens.Count; i++)
				escaped = escaped.Replace(CodeTokenPrefix + i + "__", codeTokens[i]);

			return escaped;
		}

		private static string RemoveOuterWhitespace(string value)
		{
			if (value == null)
				return string.Empty;
			return value.Trim();
		}

		private string SanitizeHtml(string html)
		{
			if (string.IsNullOrEmpty(html))
				return string.Empty;

			var withoutScripts = ScriptLikeRegex.Replace(html, string.Empty);
			var withoutComments = HtmlCommentRegex.Replace(withoutScripts, string.Empty);

			var sb = new StringBuilder();
			int lastIndex = 0;
			foreach (Match match in HtmlTagRegex.Matches(withoutComments))
			{
				sb.Append(withoutComments, lastIndex, match.Index - lastIndex);
				sb.Append(FilterTag(match));
				lastIndex = match.Index + match.Length;
			}
			sb.Append(withoutComments, lastIndex, withoutComments.Length - lastIndex);

			return sb.ToString();
		}

		private string FilterTag(Match match)
		{
			var name = match.Groups["name"].Value;
			var isClosing = match.Groups["slash"].Success;

			if (!AllowedHtmlTags.Contains(name))
				return string.Empty;

			string lowerName = name.ToLowerInvariant();
			if (isClosing)
				return "</" + lowerName + ">";

			var attrs = match.Groups["attrs"].Value;
			var allowedAttributes = AllowedHtmlAttributes.TryGetValue(lowerName, out var attrSet) ? attrSet : EmptyAttributeSet;
			var attrBuilder = new StringBuilder();

			foreach (Match attr in HtmlAttributeRegex.Matches(attrs))
			{
				var attrName = attr.Groups["name"].Value;
				if (!allowedAttributes.Contains(attrName))
					continue;

				string valueGroup = attr.Groups["value"].Success ? attr.Groups["value"].Value : string.Empty;
				string cleanedValue = StripQuotes(valueGroup);

				if (string.IsNullOrEmpty(cleanedValue))
				{
					attrBuilder.Append(' ').Append(attrName.ToLowerInvariant());
					continue;
				}

				if (IsUrlAttribute(attrName) && !IsSafeUrl(cleanedValue))
					continue;

				attrBuilder
					.Append(' ')
					.Append(attrName.ToLowerInvariant())
					.Append("=\"")
					.Append(HtmlAttributeEscape(cleanedValue))
					.Append('"');
			}

			if (SelfClosingHtmlTags.Contains(lowerName))
				return "<" + lowerName + attrBuilder + "/>";

			return "<" + lowerName + attrBuilder + ">";
		}

		private static bool IsUrlAttribute(string attrName) =>
			attrName.Equals("href", StringComparison.OrdinalIgnoreCase) ||
			attrName.Equals("src", StringComparison.OrdinalIgnoreCase);

		private static string StripQuotes(string value)
		{
			if (string.IsNullOrEmpty(value))
				return string.Empty;

			value = value.Trim();
			if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
				(value.StartsWith("'") && value.EndsWith("'")))
			{
				value = value.Substring(1, value.Length - 2);
			}
			return value;
		}

		private static bool IsSafeUrl(string url)
		{
			if (string.IsNullOrEmpty(url))
				return false;

			if (url.StartsWith("#"))
				return true;

			if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
				return absolute.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) ||
					   absolute.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

			if (Uri.TryCreate(url, UriKind.Relative, out _))
				return true;

			return false;
		}

		private static string HtmlEscape(string value)
		{
			if (string.IsNullOrEmpty(value))
				return string.Empty;

			return value
				.Replace("&", "&amp;")
				.Replace("<", "&lt;")
				.Replace(">", "&gt;")
				.Replace("\"", "&quot;")
				.Replace("'", "&#39;");
		}

		private static string HtmlAttributeEscape(string value)
		{
			if (string.IsNullOrEmpty(value))
				return string.Empty;

			return value
				.Replace("&", "&amp;")
				.Replace("\"", "&quot;")
				.Replace("<", "&lt;")
				.Replace(">", "&gt;");
		}

		private static string JsLiteral(string html)
		{
			return "'" + (html ?? string.Empty)
				.Replace("\\", "\\\\")
				.Replace("'", "\\'")
				.Replace("\n", "\\n")
				.Replace("\r", string.Empty) + "'";
		}
	}
}