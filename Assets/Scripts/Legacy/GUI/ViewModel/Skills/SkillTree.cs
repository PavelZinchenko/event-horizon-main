using System.Collections.Generic;
using System.Linq;
using Economy;
using GameServices.Gui;
using GameServices.Player;
using GameStateMachine.States;
using UnityEngine;
using UnityEngine.UI;
using Services.Audio;
using Services.Localization;
using Services.Messenger;
using Session;
using Zenject;
using Gui.Theme;

namespace ViewModel.Skills
{
    public class SkillTree : MonoBehaviour
    {
        [Inject] private readonly ISoundPlayer _soundPlayer;
        [Inject] private readonly PlayerSkills _playerSkills;
        [Inject] private readonly PlayerResources _playerResources;
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly IMessenger _messenger;
        [Inject] private readonly ISessionData _session;
        [Inject] private readonly ExitSignal.Trigger _exitTrigger;
        [Inject] private readonly GuiHelper _guiHelper;

        [SerializeField] private Transform _content;
        [SerializeField] private UiLine _linkPrefab;
        [SerializeField] private SkillTreeNode _root;
        [SerializeField] private ObjectList _nodeList;
        [SerializeField] private ToggleGroup _toggleGroup;
        [SerializeField] private InformationPanel _informationPanel;
        [SerializeField] private Text _pointsLeft;
        [SerializeField] private AudioClip _unlockSound;
        [SerializeField] private ViewModel.Common.PricePanel _resetPricePanel;
        [SerializeField] private Button _resetButton;

        public void ToggleValueChanged(bool enabled)
        {
			var node = CurrentNode;
			if (node == null)
				_informationPanel.Cleanup();
			else
			{
				var id = NodeIds[node];
				_informationPanel.Initialize(node, CanUnlockShortestPath(node), _playerSkills.HasSkill(id));
			}
        }

        public void UnlockButtonClicked()
        {
            var node = CurrentNode;
			if (node == null)
                return;

            var path = FindShortestUnlockPath(node);
            if (path == null || path.Count > _playerSkills.AvailablePoints)
                return;

            var changed = false;
            foreach (var item in path)
            {
                var id = NodeIds[item];
                if (_playerSkills.HasSkill(id))
                    continue;
                if (!_playerSkills.TryAdd(id))
                    return;

                item.State = SkillTreeNode.NodeState.EnabledAndConnected;
                UpdateLinkedNodes(item);
                changed = true;
            }

            if (!changed)
                return;

            _soundPlayer.Play(_unlockSound);
			ToggleValueChanged(true);

            UpdateResetPanel();
        }

        public void ResetSkills()
        {
            if (_playerSkills.PointsSpent == 0)
                return;

            _guiHelper.ShowConfirmation(_localization.GetString("$CommonConfirmation"), ResetSkillsImpl);
        }

        private void ResetSkillsImpl()
        {
            var price = ResetPrice;
            if (!price.TryWithdraw(_playerResources))
                return;

            _playerSkills.Reset();

            _connectedNodes.Clear();
            _toggleGroup.SetAllTogglesOff();

            RebuildTree();
            UpdateResetPanel();
            UpdateAvailablePoints();
        }

        public void Exit()
        {
            _exitTrigger.Fire();
        }

        private SkillTreeNode CurrentNode
        {
            get
            {
                var toggle = _toggleGroup.ActiveToggles().FirstOrDefault();
                return toggle ? toggle.GetComponent<SkillTreeNode>() : null;
            }
        }

        private void Start()
        {
            _messenger.AddListener(EventType.PlayerSkillsChanged, UpdateAvailablePoints);
            _messenger.AddListener(EventType.EscapeKeyPressed, OnCancel);
            UpdateAvailablePoints();
            RebuildTree();
            UpdateResetPanel();
            _informationPanel.Cleanup();
            CreateThreeBodySkillTree();
        }

        private void CreateThreeBodySkillTree()
        {
            var root = transform as RectTransform;
            if (root == null || transform.Find("Preview7SkillTabs") != null)
                return;

            var tabs = new GameObject("Preview7SkillTabs", typeof(RectTransform));
            var tabsRect = tabs.GetComponent<RectTransform>();
            tabsRect.SetParent(root, false);
            tabsRect.anchorMin = tabsRect.anchorMax = new Vector2(0.5f, 1f);
            tabsRect.pivot = new Vector2(0.5f, 1f);
            tabsRect.anchoredPosition = new Vector2(0f, -18f);
            tabsRect.sizeDelta = new Vector2(360f, 52f);

            var originalButton = CreateTreeButton(tabsRect, "原版", new Vector2(-92f, 0f));
            var threeBodyButton = CreateTreeButton(tabsRect, "三体1", new Vector2(92f, 0f));

            var panel = new GameObject("Preview7ThreeBodyTree", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.SetParent(_content.parent, false);
            if (_content is RectTransform contentRect)
            {
                panelRect.anchorMin = contentRect.anchorMin;
                panelRect.anchorMax = contentRect.anchorMax;
                panelRect.pivot = contentRect.pivot;
                panelRect.anchoredPosition = contentRect.anchoredPosition;
                panelRect.sizeDelta = contentRect.sizeDelta;
            }
            panel.GetComponent<Image>().color = UiTheme.Current.GetColor(ThemeColor.Window);
            _preview7Panel = panel;

            var line = new GameObject("RootLine", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var lineRect = line.GetComponent<RectTransform>();
            lineRect.SetParent(panelRect, false);
            lineRect.anchorMin = lineRect.anchorMax = new Vector2(0.5f, 0.5f);
            lineRect.pivot = new Vector2(0.5f, 1f);
            lineRect.anchoredPosition = new Vector2(0f, 72f);
            lineRect.sizeDelta = new Vector2(5f, 74f);
            line.GetComponent<Image>().color = UiTheme.Current.GetColor(ThemeColor.HeaderText);

            var node = new GameObject("AdvancedRadar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(Outline));
            var nodeRect = node.GetComponent<RectTransform>();
            nodeRect.SetParent(panelRect, false);
            nodeRect.anchorMin = nodeRect.anchorMax = new Vector2(0.5f, 0.5f);
            nodeRect.pivot = new Vector2(0.5f, 0.5f);
            nodeRect.anchoredPosition = new Vector2(0f, 26f);
            nodeRect.sizeDelta = new Vector2(124f, 124f);
            var originalNodeImage = _root != null ? _root.GetComponent<Image>() : null;
            var nodeImage = node.GetComponent<Image>();
            if (originalNodeImage != null)
            {
                nodeImage.sprite = originalNodeImage.sprite;
                nodeImage.type = originalNodeImage.type;
            }
            var outline = node.GetComponent<Outline>();
            outline.effectColor = UiTheme.Current.GetColor(ThemeColor.HeaderText);
            outline.effectDistance = new Vector2(3f, -3f);
            node.GetComponent<Button>().onClick.AddListener(() =>
            {
                ThreeBodySkillState.UnlockAdvancedRadar();
                UpdateAdvancedRadarNode(node);
            });

            var nodeText = CreateLabel(nodeRect, "RADAR", 20);
            nodeText.gameObject.name = "NodeLabel";
            nodeText.fontStyle = FontStyle.Bold;

            var description = new GameObject("DescriptionPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
            var descriptionRect = description.GetComponent<RectTransform>();
            descriptionRect.SetParent(panelRect, false);
            descriptionRect.anchorMin = descriptionRect.anchorMax = new Vector2(0.5f, 0.5f);
            descriptionRect.pivot = new Vector2(0.5f, 1f);
            descriptionRect.anchoredPosition = new Vector2(0f, -54f);
            descriptionRect.sizeDelta = new Vector2(390f, 108f);
            description.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.22f);
            description.GetComponent<Outline>().effectColor = UiTheme.Current.GetColor(ThemeColor.Icon);
            var descriptionText = CreateLabel(descriptionRect, string.Empty, 20);
            descriptionText.gameObject.name = "Description";
            UpdateAdvancedRadarNode(node);

            originalButton.onClick.AddListener(() => ShowThreeBodyTree(false));
            threeBodyButton.onClick.AddListener(() => ShowThreeBodyTree(true));
            ShowThreeBodyTree(false);
        }

        private void ShowThreeBodyTree(bool enabled)
        {
            _content.gameObject.SetActive(!enabled);
            if (_preview7Panel != null)
                _preview7Panel.SetActive(enabled);
            _informationPanel.gameObject.SetActive(!enabled);
        }

        private Button CreateTreeButton(RectTransform parent, string title, Vector2 position)
        {
            var go = new GameObject(title, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(170f, 46f);
            var image = go.GetComponent<Image>();
            var template = _resetButton != null ? _resetButton.GetComponent<Image>() : null;
            if (template != null)
            {
                image.sprite = template.sprite;
                image.type = template.type;
                image.color = template.color;
            }
            else
                image.color = UiTheme.Current.GetColor(ThemeColor.Window);
            CreateLabel(rect, title, 22);
            return go.GetComponent<Button>();
        }

        private static Text CreateLabel(RectTransform parent, string value, int size)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(6f, 4f);
            rect.offsetMax = new Vector2(-6f, -4f);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }

        private static void UpdateAdvancedRadarNode(GameObject node)
        {
            var unlocked = ThreeBodySkillState.AdvancedRadarUnlocked;
            node.GetComponent<Image>().color = unlocked
                ? UiTheme.Current.GetColor(ThemeColor.HeaderText)
                : UiTheme.Current.GetColor(ThemeColor.Window);
            var nodeLabel = node.transform.Find("NodeLabel")?.GetComponent<Text>();
            if (nodeLabel != null)
                nodeLabel.color = unlocked ? UiTheme.Current.GetColor(ThemeColor.Window) : UiTheme.Current.GetColor(ThemeColor.Icon);
            var text = node.transform.parent.Find("DescriptionPanel/Description")?.GetComponent<Text>();
            if (text != null)
                text.text = unlocked
                    ? "先进雷达  已解锁\n雷达范围 +20%\n舰船识别器已启用"
                    : "先进雷达\n雷达范围 +20%\n点击解锁舰船识别器";
        }

        private void RebuildTree()
        {
            foreach (var item in NodeIds)
                item.Key.State = _playerSkills.HasSkill(item.Value) ? SkillTreeNode.NodeState.Enabled : SkillTreeNode.NodeState.Disabled;
            UpdateLinkedNodes(_root);
        }

        private void UpdateResetPanel()
        {
            var price = ResetPrice;
            var isEnough = price.IsEnough(_playerResources);

            _resetPricePanel.gameObject.SetActive(price.Amount > 0);
            _resetPricePanel.Initialize(null, price, !isEnough);
            _resetButton.interactable = isEnough && _playerSkills.PointsSpent > 0;
        }

        private Price ResetPrice { get { return Economy.Price.Premium(_session.Upgrades.ResetCounter*10); } }

        private void OnCancel()
        {
            if (!this) return;
            Exit();
        }

        private void UpdateAvailablePoints()
        {
            _pointsLeft.text = _localization.GetString("$ResearchPointsAvailable", _playerSkills.AvailablePoints.ToString());
        }

        private void UpdateLinkedNodes(SkillTreeNode node)
        {
			foreach (var item in node.LinkedNodes) 
			{
				if (!_connectedNodes.Add(item))
					continue;

				if (_playerSkills.HasSkill(NodeIds[item])) 
				{
					item.State = SkillTreeNode.NodeState.EnabledAndConnected;
					UpdateLinkedNodes(item);
				}
			}
        }

        private bool CanUnlockShortestPath(SkillTreeNode target)
        {
            if (_playerSkills.HasSkill(NodeIds[target]))
                return false;

            var path = FindShortestUnlockPath(target);
            return path != null && path.Count <= _playerSkills.AvailablePoints;
        }

        private List<SkillTreeNode> FindShortestUnlockPath(SkillTreeNode target)
        {
            var sources = NodeIds.Keys
                .Where(node => node == _root ||
                               (_connectedNodes.Contains(node) && _playerSkills.HasSkill(NodeIds[node])))
                .ToArray();

            var queue = new Queue<SkillTreeNode>();
            var previous = new Dictionary<SkillTreeNode, SkillTreeNode>();
            foreach (var source in sources)
            {
                if (previous.ContainsKey(source))
                    continue;
                previous[source] = null;
                queue.Enqueue(source);
            }

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node == target)
                    break;

                foreach (var linked in node.LinkedNodes)
                {
                    if (linked == null || !NodeIds.ContainsKey(linked) || previous.ContainsKey(linked))
                        continue;
                    previous[linked] = node;
                    queue.Enqueue(linked);
                }
            }

            if (!previous.ContainsKey(target))
                return null;

            var path = new List<SkillTreeNode>();
            for (var current = target; current != null && !sources.Contains(current); current = previous[current])
                path.Add(current);
            path.Reverse();
            return path.Where(node => !_playerSkills.HasSkill(NodeIds[node])).ToList();
        }

        private Dictionary<SkillTreeNode, int> NodeIds
        {
            get
            {
                if (_nodeIds == null)
                {
                    _nodeIds = new Dictionary<SkillTreeNode, int>();
                    for (var i = 0; i < _nodeList.Children.Length; ++i)
                    {
                        var child = _nodeList.Children[i];
                        var node = child ? child.GetComponent<SkillTreeNode>() : null;
                        if (node != null)
                            _nodeIds.Add(node, i);
                    }
                }

                return _nodeIds;
            }
        }

        private Dictionary<SkillTreeNode, int> _nodeIds;
		private readonly HashSet<SkillTreeNode> _connectedNodes = new HashSet<SkillTreeNode>();
        private GameObject _preview7Panel;
    }
}
