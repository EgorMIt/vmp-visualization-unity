using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Algorithms : MonoBehaviour
{
    public ServerLogic mover;
    public UIControl ui;

    public GameObject serverPrefab;
    public GameObject standPrefab;
    public Transform serversSpawn;
    public Transform standsSpawn;

    public int standSpace = 10;

    public int[] servers;
    public int[] stands;
    public GameObject[] serverObjs;

    public AudioSource audioPlayer;
    public AudioClip audioClip1;
    public AudioClip audioClip2;

    public int currentServer = 0;
    private int[] _freeSpace;
    private bool[] _notPlaced;

    public int[,] sortedGenetic;
    private int[] _geneticCrnt;
    private int[] _geneticSpace;
    private int[,,] _generation;
    private int[,] _generationSpace;
    private int[,] _generationCrnt;

    public int[,] sortedAnnealing;
    private int[] _annealingCrnt;
    private int[] _annealingSpace;

    public int[,] sortedNeighbour;
    private int[] _neighbourCrnt;
    private int[] _neighbourSpace;

    public float standInbetween = 2.2f;

    public float temperature = 100.0f;
    public float startTemperature = 100.0f;
    public float coolingRate = 0.1f;
    public int currentEnergy;

    public int population = 12;
    public int maxIterations = 100;
    public int currentIteration = 0;
    public float mutationRate = 0.5f;

    private void Start()
    {
        maxIterations = 50;
    }

    public void Init()
    {
        _freeSpace = new int[stands.Length];
        _notPlaced = new bool[servers.Length];

        _geneticSpace = new int[stands.Length];
        sortedGenetic = new int[stands.Length, standSpace];
        _geneticCrnt = new int[stands.Length];

        _annealingSpace = new int[stands.Length];
        sortedAnnealing = new int[stands.Length, standSpace];
        _annealingCrnt = new int[stands.Length];

        _neighbourSpace = new int[stands.Length];
        sortedNeighbour = new int[stands.Length, standSpace];
        _neighbourCrnt = new int[stands.Length];

        _generation = new int[population, stands.Length, standSpace];
        _generationSpace = new int[population, stands.Length];
        _generationCrnt = new int[population, stands.Length];

        for (var i = 0; i < stands.Length; i++)
        {
            _freeSpace[i] = standSpace;
            _geneticSpace[i] = standSpace;
            _annealingSpace[i] = standSpace;
            _neighbourSpace[i] = standSpace;
            for (int j = 0; j < population; j++)
                _generationSpace[j, i] = standSpace;
        }

        GetRandomState();
        GetRandomGeneration();
    }

    public IEnumerator SortFirstFit()
    {
        for (var server = currentServer; server < servers.Length; server++)
        {
            yield return StartCoroutine(SortFirstFitOne());
        }
    }

    public IEnumerator SortFirstFitOne()
    {
        if (currentServer >= servers.Length) yield break;
        ui.DisableUI();
        for (var server = currentServer + 1; server < servers.Length; server++)
        {
            serverObjs[server].GetComponent<ServerBlock>().MoveForward(standInbetween);
        }

        //audioPlayer.PlayOneShot(audioClip1);
        for (var stand = 0; stand < stands.Length; stand++)
        {
            if (_freeSpace[stand] >= servers[currentServer])
            {
                Vector3 moveTo = standsSpawn.position;
                moveTo.z = serversSpawn.position.z;
                moveTo.x += standInbetween * stand;
                mover.MoveServerTo(serverObjs[currentServer].transform, moveTo, 1);
                yield return new WaitForSeconds(1);
                moveTo.y = 0.3f * (standSpace - _freeSpace[stand]);
                mover.MoveServerTo(serverObjs[currentServer].transform, moveTo, 1);
                yield return new WaitForSeconds(1);
                moveTo.z = standsSpawn.position.z;
                mover.MoveServerTo(serverObjs[currentServer].transform, moveTo, 1);
                yield return new WaitForSeconds(1);

                _freeSpace[stand] -= servers[currentServer];
                currentServer++;
                break;
            }
        }

        ui.EnableUI();
    }

    public IEnumerator SortBestFit()
    {
        for (var server = currentServer; server < servers.Length; server++)
        {
            yield return StartCoroutine(SortBestFitOne());
        }
    }

    public IEnumerator SortBestFitOne()
    {
        if (currentServer >= servers.Length) yield break;
        ui.DisableUI();
        for (var server = currentServer + 1; server < servers.Length; server++)
        {
            serverObjs[server].GetComponent<ServerBlock>().MoveForward(standInbetween);
        }

        var bestFit = -1;
        for (var stand = 0; stand < stands.Length; stand++)
        {
            if (_freeSpace[stand] >= servers[currentServer])
            {
                if (bestFit == -1)
                    bestFit = stand;
                else if (_freeSpace[bestFit] > _freeSpace[stand])
                    bestFit = stand;
            }
        }

        //audioPlayer.PlayOneShot(audioClip1);
        if (bestFit != -1)
        {
            Vector3 moveTo = standsSpawn.position;
            moveTo.z = serversSpawn.position.z;
            moveTo.x += standInbetween * bestFit;
            mover.MoveServerTo(serverObjs[currentServer].transform, moveTo, 1);
            yield return new WaitForSeconds(1);
            moveTo.y = 0.3f * (standSpace - _freeSpace[bestFit]);
            mover.MoveServerTo(serverObjs[currentServer].transform, moveTo, 1);
            yield return new WaitForSeconds(1);
            moveTo.z = standsSpawn.position.z;
            mover.MoveServerTo(serverObjs[currentServer].transform, moveTo, 1);
            yield return new WaitForSeconds(1);

            _freeSpace[bestFit] -= servers[currentServer];
            currentServer++;
        }

        ui.EnableUI();
    }

    public IEnumerator SortSimAnnealing()
    {
        while (temperature > 1)
        {
            yield return StartCoroutine(SortSimAnnealingOne());
        }
    }

    public IEnumerator SortSimAnnealingOne()
    {
        if (temperature < 1) yield break;
        ui.DisableUI();
        currentServer = servers.Length;
        currentEnergy = GetCurrentEnergy();
        int newEnergy;
        do
        {
            GetRandomNeighbour();
            newEnergy = GetCurrentEnergy();

            var deltaEnergy = newEnergy - currentEnergy;
            if (deltaEnergy < 0)
            {
                CopyToAnnealing();
                currentEnergy = newEnergy;
            }
            else
            {
                double acceptanceProbability = Mathf.Exp(-deltaEnergy / temperature);
                if (Random.Range(0.0f, 1.0f) < acceptanceProbability)
                {
                    CopyToAnnealing();
                    currentEnergy = newEnergy;
                }
            }

            temperature *= 1 - coolingRate;
        } while (newEnergy != currentEnergy);

        //audioPlayer.PlayOneShot(audioClip2);

        for (var stand = 0; stand < stands.Length; stand++)
        {
            var crntPos = 0;
            for (var server = 0; server < _annealingCrnt[stand]; server++)
            {
                Vector3 moveTo = standsSpawn.position;
                moveTo.x += standInbetween * stand;
                moveTo.y = 0.3f * crntPos;
                serverObjs[sortedAnnealing[stand, server]].transform.position = moveTo;
                crntPos += servers[sortedAnnealing[stand, server]];
            }
        }

        yield return new WaitForSeconds(0.5f);
        ui.EnableUI();
    }

    public IEnumerator SortGenetic()
    {
        for (var iteration = currentIteration; iteration < maxIterations; iteration++)
        {
            yield return StartCoroutine(SortGeneticOne());
        }
    }

    public IEnumerator SortGeneticOne()
    {
        if (currentIteration >= maxIterations) yield break;
        ui.DisableUI();
        currentServer = servers.Length;

        MutateGeneration();
        CopyBest();

        //audioPlayer.PlayOneShot(audioClip2);

        for (var stand = 0; stand < stands.Length; stand++)
        {
            var crntPos = 0;
            for (var server = 0; server < _geneticCrnt[stand]; server++)
            {
                Vector3 moveTo = standsSpawn.position;
                moveTo.x += standInbetween * stand;
                moveTo.y = 0.3f * crntPos;
                serverObjs[sortedGenetic[stand, server]].transform.position = moveTo;
                crntPos += servers[sortedGenetic[stand, server]];
            }
        }

        currentIteration++;
        yield return new WaitForSeconds(0.5f);
        ui.EnableUI();
    }

    public void Reset()
    {
        temperature = startTemperature;
        currentIteration = 0;
        for (var server = 0; server < serverObjs.Length; server++)
        {
            Vector3 tmpPos = serversSpawn.position;
            tmpPos.x -= server * standInbetween;
            serverObjs[server].transform.position = tmpPos;
            currentServer = 0;
        }

        for (var i = 0; i < stands.Length; i++)
        {
            _geneticCrnt[i] = 0;
            _annealingCrnt[i] = 0;
            _neighbourCrnt[i] = 0;

            _freeSpace[i] = standSpace;
            _geneticSpace[i] = standSpace;
            _annealingSpace[i] = standSpace;
            _neighbourSpace[i] = standSpace;
            for (var j = 0; j < population; j++)
            {
                _generationCrnt[j, i] = 0;
                _generationSpace[j, i] = standSpace;
            }
        }

        GetRandomState();
        GetRandomGeneration();
        ui.algorithms.SetEnabled(true);
    }

    private void GetRandomState()
    {
        for (var server = 0; server < servers.Length; server++)
        {
            var stand = Random.Range(0, stands.Length);
            var count = 10;
            while (_annealingSpace[stand] < servers[server] && count > 0)
            {
                stand = Random.Range(0, stands.Length);
                count--;
            }

            if (_annealingSpace[stand] < servers[server])
            {
                _notPlaced[server] = true;
                continue;
            }

            _annealingSpace[stand] -= servers[server];
            sortedAnnealing[stand, _annealingCrnt[stand]] = server;
            _annealingCrnt[stand]++;
        }

        CopyToNeighbour();
    }

    private void GetRandomNeighbour()
    {
        CopyToNeighbour();
        for (var stand = 0; stand < stands.Length; stand++)
        {
            for (var server = 0; server < _neighbourCrnt[stand]; server++)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    var newStand = Random.Range(0, stands.Length);
                    var count = 10;
                    while (_neighbourSpace[newStand] < servers[sortedNeighbour[stand, server]] && count > 0)
                    {
                        newStand = Random.Range(0, stands.Length);
                        count--;
                    }

                    if (_neighbourSpace[newStand] < servers[sortedNeighbour[stand, server]])
                        continue;
                    sortedNeighbour[newStand, _neighbourCrnt[newStand]] = sortedNeighbour[stand, server];
                    _neighbourSpace[newStand] -= servers[sortedNeighbour[stand, server]];
                    _neighbourCrnt[newStand]++;
                    _neighbourSpace[stand] += servers[sortedNeighbour[stand, server]];
                    _neighbourCrnt[stand]--;
                    for (int i = server; i < _neighbourCrnt[stand]; i++)
                    {
                        sortedNeighbour[stand, i] = sortedNeighbour[stand, i + 1];
                    }
                }
            }
        }

        for (var server = 0; server < servers.Length; server++)
        {
            if (_notPlaced[server])
            {
                int stand = Random.Range(0, stands.Length);
                int count = stands.Length * 2;
                while (_annealingSpace[stand] < servers[server] && count > 0)
                {
                    stand = Random.Range(0, stands.Length);
                    count--;
                }

                if (_annealingSpace[stand] < servers[server])
                    continue;
                _annealingSpace[stand] -= servers[server];
                sortedAnnealing[stand, _annealingCrnt[stand]] = server;
                _annealingCrnt[stand]++;
                _notPlaced[server] = false;
            }
        }
    }

    private int GetCurrentEnergy()
    {
        int energy = 0;
        for (int stand = 0; stand < stands.Length; stand++)
        {
            energy += (stand + 5) * _neighbourSpace[stand];
        }

        return energy;
    }

    private void CopyToAnnealing()
    {
        _neighbourSpace.CopyTo(_annealingSpace, 0);
        _neighbourCrnt.CopyTo(_annealingCrnt, 0);
        for (int stand = 0; stand < stands.Length; stand++)
        {
            for (int server = 0; server < standSpace; server++)
            {
                sortedAnnealing[stand, server] = sortedNeighbour[stand, server];
            }
        }
    }

    private void CopyToNeighbour()
    {
        _annealingSpace.CopyTo(_neighbourSpace, 0);
        _annealingCrnt.CopyTo(_neighbourCrnt, 0);
        for (int stand = 0; stand < stands.Length; stand++)
        {
            for (int server = 0; server < standSpace; server++)
            {
                sortedNeighbour[stand, server] = sortedAnnealing[stand, server];
            }
        }
    }

    private void GetRandomGeneration()
    {
        for (int one = 0; one < population; one++)
        {
            for (int server = 0; server < servers.Length; server++)
            {
                int stand = Random.Range(0, stands.Length);
                int count = 50;
                while (_generationSpace[one, stand] < servers[server] && count > 0)
                {
                    stand = Random.Range(0, stands.Length);
                    count--;
                }

                if (_generationSpace[one, stand] < servers[server])
                {
                    _notPlaced[server] = true;
                    continue;
                }

                _generationSpace[one, stand] -= servers[server];
                _generation[one, stand, _generationCrnt[one, stand]] = server;
                _generationCrnt[one, stand]++;
            }
        }
    }

    private void MutateGeneration()
    {
        for (int one = Random.Range(0, 1); one < population - 4; one += 4)
        {
            int parent1, parent2;
            int child1, child2;
            if (GetCurrentScore(one) > GetCurrentScore(one + 2))
            {
                parent1 = one;
                child1 = one + 2;
            }
            else
            {
                parent1 = one + 2;
                child1 = one;
            }

            if (GetCurrentScore(one + 1) > GetCurrentScore(one + 3))
            {
                parent2 = one + 1;
                child2 = one + 3;
            }
            else
            {
                parent2 = one + 3;
                child2 = one + 1;
            }

            RandomMutation(parent1, child1);
            RandomMutation(parent2, child2);
        }
    }

    private void RandomMutation(int parent, int child)
    {
        for (var stand = 0; stand < stands.Length; stand++)
        {
            _generationSpace[child, stand] = _generationSpace[parent, stand];
            _generationCrnt[child, stand] = _generationCrnt[parent, stand];
            for (int server = 0; server < standSpace; server++)
            {
                _generation[child, stand, server] = _generation[parent, stand, server];
            }
        }

        for (var stand = 0; stand < stands.Length; stand++)
        {
            for (var server = 0; server < _generationCrnt[child, stand]; server++)
            {
                if (Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    var newStand = Random.Range(0, stands.Length);
                    var count = 10;
                    while (_generationSpace[child, newStand] < servers[_generation[child, stand, server]] && count > 0)
                    {
                        newStand = Random.Range(0, stands.Length);
                        count--;
                    }

                    if (_generationSpace[child, newStand] < servers[_generation[child, stand, server]])
                        continue;
                    _generation[child, newStand, _generationCrnt[child, newStand]] = _generation[child, stand, server];
                    _generationSpace[child, newStand] -= servers[_generation[child, stand, server]];
                    _generationCrnt[child, newStand]++;
                    _generationSpace[child, stand] += servers[_generation[child, stand, server]];
                    _generationCrnt[child, stand]--;
                    for (var i = server; i < _generationCrnt[child, stand]; i++)
                    {
                        _generation[child, stand, i] = _generation[child, stand, i + 1];
                    }
                }
            }
        }
    }

    private void CopyBest()
    {
        var bestId = GetBest();

        for (var stand = 0; stand < stands.Length; stand++)
        {
            _geneticSpace[stand] = _generationSpace[bestId, stand];
            _geneticCrnt[stand] = _generationCrnt[bestId, stand];
            for (var server = 0; server < standSpace; server++)
            {
                sortedGenetic[stand, server] = _generation[bestId, stand, server];
            }
        }
    }

    private int GetCurrentScore(int one)
    {
        var score = 0;
        for (var stand = 0; stand < stands.Length; stand++)
        {
            score += (stand + 5) * _generationSpace[one, stand];
        }

        return score;
    }

    private int GetBest()
    {
        var best = 0;
        var bestId = 0;
        for (var one = 0; one < population; one++)
        {
            var current = GetCurrentScore(one);
            if (current >= best)
            {
                best = current;
                bestId = one;
            }
        }

        return bestId;
    }
}