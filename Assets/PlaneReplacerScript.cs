using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlaneReplacerScript : MonoBehaviour
{
    [SerializeField] private TextAsset ReadmeAsset;

    private string NachsaveName = "NACHSAVE";
    private string NachsavePath;
    private string FolderName = "REPLACER";
    private string FolderPath;
    private string TsvName = "REPLACER.TSV";
    private string TsvPath;
    private string ReadmeName = "README.TXT";
    private string ReadmePath;

    private void Start()
    {
        // Create directories. Does nothing if directories already exist.
        NachsavePath = Path.Combine(Application.persistentDataPath, NachsaveName);
        Directory.CreateDirectory(NachsavePath);
        FolderPath = Path.Combine(NachsavePath, FolderName);
        Directory.CreateDirectory(FolderPath);

        TsvPath = Path.Combine(FolderPath, TsvName);
        if (!File.Exists(TsvPath))
        {
            File.WriteAllText(TsvPath, "");
        }

        ReadmePath = Path.Combine(FolderPath, ReadmeName);
        if (!File.Exists(ReadmePath))
        {
            File.WriteAllText(ReadmePath, ReadmeAsset.text);
        }

        ReplaceFiles();

        ServiceProvider.Instance.DevConsole.RegisterCommand("ReplaceAircraftFiles", ReplaceFiles);
    }

    private void ReplaceFiles()
    {
        string[] lines;
        int passed = 0;
        int total = 0;

        if (!File.Exists(TsvPath))
        {
            File.Create(TsvPath);
        }

        try
        {
            lines = File.ReadAllLines(TsvPath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to open REPLACER.TSV: " + ex.Message);
            return;
        }

        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("REPLACER.TSV is empty.");
            return;
        }

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] values = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            total++;

            if (values.Length < 2)
            {
                Debug.LogWarning("Cannot replace: Only one file path was provided.");
                continue;
            }

            string clearPath = Path.Combine(Application.persistentDataPath, "AircraftDesigns", values[0]);
            string copyPath = Path.Combine(Application.persistentDataPath, "AircraftDesigns", values[1]);

            try
            {
                string aircraftData = File.ReadAllText(copyPath);
                File.WriteAllText(clearPath, aircraftData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write {copyPath} to {clearPath}: " + ex.Message);
                continue;
            }

            // The message is printed if successful
            Debug.Log($"Successfully wrote {copyPath} to {clearPath}");
            passed++;
        }

        Debug.Log($"Completed ReplaceAircraftFiles ({passed} of {total} operations successful)");
    }
}
