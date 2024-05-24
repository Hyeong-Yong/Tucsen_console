using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using TUCAMAPI;

namespace C.NET04_prop_list
{
    class Program
    {
        public static TUCAM_INIT m_itApi;       // SDK API initialized object
        public static TUCAM_OPEN m_opCam;       // Open camera object

        public static string[] m_strArrProp = { "TUIDP_GLOBALGAIN      : [Global Gain]                ", 
                                                "TUIDP_EXPOSURETM      : [Exposure Time]              ",
                                                "TUIDP_BRIGHTNESS      : [Brightness(Auto Exposure)]  ",
                                                "TUIDP_BLACKLEVEL      : [Black Level]                ",
                                                "TUIDP_TEMPERATURE     : [Temperature]                ", 
                                                "TUIDP_SHARPNESS       : [Sharpness]                  ", 
                                                "TUIDP_NOISELEVEL      : [Noise Level]                ", 
                                                "TUIDP_HDR_KVALUE      : [HDR K Value]                ",
                                                "TUIDP_GAMMA           : [Gamma]                      ",
                                                "TUIDP_CONTRAST        : [Contrast]                   ",
                                                "TUIDP_LFTLEVELS       : [Left Levels]                ",
                                                "TUIDP_RGTLEVELS       : [Right Levels]               ",
                                                "TUIDP_CHNLGAIN        : [Channel Gain]               ",
                                                "TUIDP_SATURATION      : [Saturation]                 ",
                                                "TUIDP_CLRTEMPERATURE  : [Color Temperature]          ",
                                                "TUIDP_CLRMATRIX       : [Color Matrix]               ",
                                                "TUIDP_DPCLEVEL        : [DPC Level]                  ",
                                                "TUIDP_BLACKLEVELHG    : [Black Level High Gain]      ",
                                                "TUIDP_BLACKLEVELLG    : [Black Level Low Gain]       ",                                                
                                                "TUIDP_POWEEFREQUENCY  : [Power frequency]            ",
                                                "TUIDP_HUE             : [Hue]                        ",
                                                "TUIDP_LIGHT           : [Light]                      ",
                                                "TUIDP_ENHANCE_STRENGTH: [Enhance strength]           ",
                                                "TUIDP_NOISELEVEL_3D   : [Noise Level 3D]             ",
                                                "TUIDP_FOCUS_POSITION  : [Position]                   ",
                                                "TUIDP_FRAME_RATE      : [Frame Rate]                 ",
                                                "TUIDP_START_TIME      : [Start Time]                 ",
                                                "TUIDP_FRAME_NUMBER    : [Frame Number]               ",
                                                "TUIDP_INTERVAL_TIME   : [Interval Time]              ",
                                                "TUIDP_GPS_APPLY       : [GPS Apply]                  ",
                                                "TUIDP_AMB_TEMPERATURE : [Amb Temperature]            ",                                                
                                                "TUIDP_AMB_HUMIDITY    : [Amb Humidity]               ",
                                                "TUIDP_AUTO_CTRLTEMP   : [Auto Control Temperature]   ",                                                         
                                                "TUIDP_AVERAGEGRAY     : [Average]                    ",
                                                "TUIDP_AVERAGEGRAYTHD  : [Average Gray Thd]           ",
                                                "TUIDP_ENHANCETHD      : [Enhance Threshold ]         ",
                                                "TUIDP_ENHANCEPARA     : [Enhance Parameter]          ",
                                                "TUIDP_EXPOSUREMAX     : [Exposure Max]               ",
                                                "TUIDP_EXPOSUREMIN     : [Exposure Min]               ",                                                
                                                "TUIDP_GAINMAX         : [Gain Max]                   ",
                                                "TUIDP_GAINMIN         : [Gain Min]                   ",
                                                "TUIDP_THROUGHFOGPARA  : [Through Fog]                ",
                                                "TUIDP_ATLEVEL_PERCENTAGE   : [Auto Level Ignore]     ",
                                                "TUIDP_TEMPERATURE_TARGET   : [Temperature Target]    ",
                                                "TUIDP_PIXELRATIO      : [pixel ratio]                ",

                                              };

        /* Init the TUCAM API */
        static TUCAMRET InitApi()
        {
            /* Get the current directory */
            IntPtr strPath = Marshal.StringToHGlobalAnsi(System.Environment.CurrentDirectory);

            m_itApi.uiCamCount = 0;
            m_itApi.pstrConfigPath = strPath;

            TUCamAPI.TUCAM_Api_Init(ref m_itApi);

            Console.WriteLine("Connect {0} camera", m_itApi.uiCamCount);

            if (0 == m_itApi.uiCamCount)
            {
                return TUCAMRET.TUCAMRET_NO_CAMERA;
            }

            return TUCAMRET.TUCAMRET_SUCCESS;
        }

        /* UnInit the TUCAM API */
        static TUCAMRET UnInitApi()
        {
            return TUCamAPI.TUCAM_Api_Uninit();
        }

        /* Open the camera by index number */
        static TUCAMRET OpenCamera(uint uiIdx)
        {
            if (uiIdx >= m_itApi.uiCamCount)
            {
                return TUCAMRET.TUCAMRET_OUT_OF_RANGE;
            }

            m_opCam.uiIdxOpen = uiIdx;

            return TUCamAPI.TUCAM_Dev_Open(ref m_opCam);
        }

        /* Close the current camera */
        static TUCAMRET CloseCamera()
        {
            if (null != m_opCam.hIdxTUCam)
            {
                TUCamAPI.TUCAM_Dev_Close(m_opCam.hIdxTUCam);
            }

            Console.WriteLine("Close the camera success");

            return TUCAMRET.TUCAMRET_SUCCESS;
        }
                
        /* Print the camera property list */
        static void PrintCameraPropertyList()
        {
            double dbVal = 0;

            List<string> lstProp = new List<string>(m_strArrProp);

            TUCAM_PROP_ATTR prop;
            prop.idProp = 0;
            prop.nIdxChn = 0;
            prop.dbValDft = 0;
            prop.dbValMin = 0;
            prop.dbValMax = 0;
            prop.dbValStep = 0;

            /* Get property list information */
            Console.WriteLine("Get property information list");

            for (int i = (int)TUCAM_IDPROP.TUIDP_GLOBALGAIN; i < (int)TUCAM_IDPROP.TUIDP_ENDPROPERTY; ++i)
            {
                prop.idProp = i;

                if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Prop_GetAttr(m_opCam.hIdxTUCam, ref prop))
                {
                    Console.WriteLine("0x{0:X2}.{1} Range[{2, 5:f2}, {3, 8:f2}] Default:{4, 6:f2} Step:{5, 6:f2}", i, lstProp[i].ToString(), prop.dbValMin, prop.dbValMax, prop.dbValDft, prop.dbValStep);
                }
                else
                {
                    Console.WriteLine("0x{0:X2}.{1} Not support", i, lstProp[i].ToString());
                }
            }

            Console.WriteLine();

            /* Set property default value */
            Console.WriteLine("Set property default value");
            for (int i = (int)TUCAM_IDPROP.TUIDP_GLOBALGAIN; i < (int)TUCAM_IDPROP.TUIDP_ENDPROPERTY; ++i)
            {
                prop.idProp = i;
                prop.nIdxChn = 0;


                if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Prop_GetAttr(m_opCam.hIdxTUCam, ref prop))
                {

                    if ((int)TUCAM_IDPROP.TUIDP_CLRTEMPERATURE == i 
                        || (int)TUCAM_IDPROP.TUIDP_HDR_KVALUE == i
                        || (int)TUCAM_IDPROP.TUIDP_BLACKLEVELHG == i
                        || (int)TUCAM_IDPROP.TUIDP_BLACKLEVELLG == i)    /* Color Temperature*/ 
                    {
                        // Color temperature change the RGB value
                        continue;
                    }
                    else if ((int)TUCAM_IDPROP.TUIDP_CHNLGAIN == i)     /* Channels Gain */
                    {
                        // Close the auto white balance state
                        TUCamAPI.TUCAM_Capa_SetValue(m_opCam.hIdxTUCam, (int)TUCAM_IDCAPA.TUIDC_ATWBALANCE, 0);

                        // Red
                        TUCamAPI.TUCAM_Prop_SetValue(m_opCam.hIdxTUCam, i, prop.dbValDft, 1);
                        Console.WriteLine("0x{0:X2}.{1} Set default value {2, 6:f2} success (channel red) ", i, lstProp[i].ToString(), prop.dbValDft);

                        // Green
                        TUCamAPI.TUCAM_Prop_SetValue(m_opCam.hIdxTUCam, i, prop.dbValDft, 2);
                        Console.WriteLine("0x{0:X2}.{1} Set default value {2, 6:f2} success (channel green)", i, lstProp[i].ToString(), prop.dbValDft);

                        // Blue
                        TUCamAPI.TUCAM_Prop_SetValue(m_opCam.hIdxTUCam, i, prop.dbValDft, 3);
                        Console.WriteLine("0x{0:X2}.{1} Set default value {2, 6:f2} success (channel blue)", i, lstProp[i].ToString(), prop.dbValDft);
                    }
                    else
                    {
                        TUCamAPI.TUCAM_Prop_SetValue(m_opCam.hIdxTUCam, i, prop.dbValDft, 0);
                        Console.WriteLine("0x{0:X2}.{1} Set default value {2, 6:f2} success", i, lstProp[i].ToString(), prop.dbValDft);
                    }
                }
            }

            Console.WriteLine();

            /* Get property current value */
            Console.WriteLine("Get property current value");
            for (int i = (int)TUCAM_IDPROP.TUIDP_GLOBALGAIN; i < (int)TUCAM_IDPROP.TUIDP_ENDPROPERTY; ++i)
            {
                // Channels Gain
                if ((int)TUCAM_IDPROP.TUIDP_CHNLGAIN == i)
                {
                    // Red
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Prop_GetValue(m_opCam.hIdxTUCam, i, ref dbVal, 1))
                    {
                        Console.WriteLine("0x{0:X2}.{1} The current value is {2, 6:f2} (channel red)", i, lstProp[i].ToString(), dbVal);
                    }

                    // Green
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Prop_GetValue(m_opCam.hIdxTUCam, i, ref dbVal, 2))
                    {
                        Console.WriteLine("0x{0:X2}.{1} The current value is {2, 6:f2} (channel green)", i, lstProp[i].ToString(), dbVal);
                    }

                    // Blue
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Prop_GetValue(m_opCam.hIdxTUCam, i, ref dbVal, 3))
                    {
                        Console.WriteLine("0x{0:X2}.{1} The current value is {2, 6:f2} (channel blue)", i, lstProp[i].ToString(), dbVal);
                    }
                }
                else
                {
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Prop_GetValue(m_opCam.hIdxTUCam, i, ref dbVal, 0))
                    {
                        Console.WriteLine("0x{0:X2}.{1} The current value is {2, 6:f2}", i, lstProp[i].ToString(), dbVal);
                    }
                }                                 
            }

            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            if (TUCAMRET.TUCAMRET_SUCCESS == InitApi())
            {
                if (TUCAMRET.TUCAMRET_SUCCESS == OpenCamera(0))
                {
                    Console.WriteLine("Open the camera success");

                    Console.WriteLine();
                    PrintCameraPropertyList();
                    Console.WriteLine();

                    CloseCamera();
                }
                else
                {
                    Console.WriteLine("Open the camera failure");
                }

                UnInitApi();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
