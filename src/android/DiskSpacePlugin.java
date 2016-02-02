package com.sqli.cordova.diskSpace;

import android.os.Environment;
import android.os.StatFs;
import android.util.Log;

import org.apache.cordova.CallbackContext;
import org.apache.cordova.CordovaInterface;
import org.apache.cordova.CordovaPlugin;
import org.apache.cordova.CordovaWebView;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.File;

public class DiskSpacePlugin extends CordovaPlugin {
    public static final String TAG = "DiskSpace Plugin";

    /**
     * Constructor.
     */
    public DiskSpacePlugin() {
    }

    /**
     * Sets the context of the Command. This can then be used to do things like
     * get file paths associated with the Activity.
     *
     * @param cordova The context of the main Activity.
     * @param webView The CordovaWebView Cordova is running in.
     */
    public void initialize(CordovaInterface cordova, CordovaWebView webView) {
        super.initialize(cordova, webView);
        Log.v(TAG, "Initialisation DiskSpacePlugin");
    }

    public boolean execute(final String action, JSONArray args,
                           CallbackContext callbackContext) throws JSONException {

        int location = 1;
        try {
            location = args.getJSONObject(0).getInt("location");
        } catch (JSONException e) {

        }

        if ("info".equals(action)) {
            StatFs statFs = null;

            String packageName = this.cordova.getActivity().getPackageName();
            File appDir = null;

            if (location == 1) {
                statFs = new StatFs(Environment.getExternalStorageDirectory().getAbsolutePath());
                appDir = new File(Environment.getExternalStorageDirectory().getAbsolutePath() + File.separator + "Android" + File.separator + "data" + File.separator + packageName + File.separator + "files");
            } else {
                statFs = new StatFs(Environment.getDataDirectory().getAbsolutePath());
                appDir = new File(Environment.getDataDirectory().getAbsolutePath() + File.separator + "data" + File.separator + packageName + File.separator + "files");
            }

            JSONObject objRes = new JSONObject();
            objRes.put("app", getFolderSize(appDir));
            objRes.put("total", statFs.getTotalBytes());
            objRes.put("free", statFs.getFreeBytes());
            callbackContext.success(objRes);
        }

        return true;
    }

    private static long getFolderSize(File f) {
        long size = 0;
        if (f.isDirectory()) {
            for (File file : f.listFiles()) {
                size += getFolderSize(file);
            }
        } else {
            size = f.length();
        }
        return size;
    }
}
