using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfigServers : MonoBehaviour
{
    public Algorithms main;
    public UIControl hud;
    private int _serversCount, _standsCount;
    private SliderInt[] _servers;

    private VisualElement _root;
    private ScrollView _view;
    private Button _confirm;

    private void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _view = _root.Q<ScrollView>("list");
        _confirm = _root.Q<Button>("confirm");
        _confirm.clicked += () => Confirm();
        _root.visible = false;
    }


    private void Update()
    {
        for (var i = 0; i < _serversCount; i++)
        {
            _servers[i].label = "[№ " + (i + 1) + "] - " + _servers[i].value;
        }

        if (_serversCount == 0) return;

        var serversTotal = _servers.Aggregate(0, (current, t) => current * t.value);

        // for (var i = 0; i < _servers.Length; i++)
        // {
        //     serversTotal += _servers[i].value;
        // }

        _confirm.SetEnabled(serversTotal <= _standsCount * 10);
    }

    public void Init(int stand, int server)
    {
        _root.visible = true;
        _serversCount = server;
        _standsCount = stand;
        _servers = new SliderInt[_serversCount];

        // VisualElement a = _root.Q<VisualElement>("view");
        //view.Q<VisualElement>("unity-content-viewport").pickingMode = PickingMode.Ignore;
        _view.pickingMode = PickingMode.Ignore;
        _view.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;

        //view.verticalScroller.pickingMode = PickingMode.Ignore;
        //view.verticalPageSize = 10;

        for (var i = 0; i < _serversCount; i++)
        {
            _servers[i] = new SliderInt
            {
                label = "[# " + (i + 1) + "]",
                value = Random.Range(1, 10),
                lowValue = 1,
                highValue = 10,
                // choices = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" }
            };
            _view.Add(_servers[i]);
            //servers[i].pickingMode = PickingMode.Ignore;
            _servers[i].style.alignItems = Align.Stretch;
            _servers[i].style.width = Length.Percent(80);
            _servers[i].style.left = Length.Percent(5);
            // servers[i].style.unityTextAlign = TextAnchor.MiddleCenter;
        }
    }

    private void Confirm()
    {
        Vector3 tmpPos = main.standsSpawn.position;

        main.stands = new int[_standsCount];

        for (var stand = 0; stand < _standsCount; stand++)
        {
            Instantiate(main.standPrefab, tmpPos, main.standsSpawn.rotation);
            tmpPos.x += main.standInbetween;
        }

        main.servers = new int[_serversCount];
        main.serverObjs = new GameObject[_serversCount];
        tmpPos = main.serversSpawn.position;
        for (var server = 0; server < _serversCount; server++)
        {
            GameObject tmp = Instantiate(main.serverPrefab, tmpPos, main.serversSpawn.rotation);
            var count = _servers[server].value;
            tmp.GetComponent<ServerBlock>().Init(count);
            main.servers[server] = count;
            main.serverObjs[server] = tmp;
            tmpPos.x -= main.standInbetween;
        }

        _root.visible = false;
        main.Init();
        hud.canControl = true;
    }
}