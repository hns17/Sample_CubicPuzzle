using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

/**
 *  @brief  Task CancellationToken Manager
 */

public class CTSManager
{
    private CancellationTokenSource defaultCancellationTokenSource = null;
    private Dictionary<string, CancellationTokenSource> ctss;
    
    /**
     *  @brief  CancellationTokenSource 반환
     *  @param  sourceName(string) : key
     *  @return CancellationTokenSource
     */
    public CancellationTokenSource GetCancellationTokenSource(string sourceName)
    {
        CancellationTokenSource getSource = null;

        if(ctss == null) {
            ctss = new Dictionary<string, CancellationTokenSource>();
        }
        else {
            ctss.TryGetValue(sourceName, out getSource);
        }

        //cts가 없는 경우 생성 후 반환
        if(getSource == null) {
            getSource = new CancellationTokenSource();
            ctss[sourceName] = getSource;
        }

        return getSource;
    }

    /**
     *  @brief  CancellationTokenSource 반환
     *  @param  sourceName(string) : key
     *  @return CancellationTokenSource
     */
    public CancellationTokenSource GetDefaultCancellationTokenSource()
    {
       if(defaultCancellationTokenSource == null) {
            defaultCancellationTokenSource = new CancellationTokenSource();
        }
        return defaultCancellationTokenSource;
    }

    /**
     *  @brief  Cancel CancellationToken
     *  @param  sourceName(string) : key
     */
    public void Cancellation(string sourceName)
    {
        CancellationTokenSource getSource = null;
        if(ctss.TryGetValue(sourceName, out getSource)) {
            getSource?.Cancel();

            ctss.Remove(sourceName);
        }
    }

    /**
     *  @brief  Cancel All Token
     */
    public void CancellationAll()
    {
        defaultCancellationTokenSource.Cancel();
        defaultCancellationTokenSource = null;

        if(ctss != null) {
            foreach(var item in ctss) {
                item.Value?.Cancel();
            }

            ctss.Clear();
        }
    }
}
