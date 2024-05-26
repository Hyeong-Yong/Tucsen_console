using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TUCAMAPI;

namespace C.NET03_capa_list {
    class Program {
        public static TUCAM_INIT m_itApi;       // SDK API initialized object
        public static TUCAM_OPEN m_opCam;       // Open camera object

        public static string[] m_strArrCapa = { "TUIDC_RESOLUTION          : [Resolution]                 ",
                                                "TUIDC_PIXELCLOCK          : [Pixel Clock]                ",
                                                "TUIDC_BITOFDEPTH          : [Bit Of Depth]               ",
                                                "TUIDC_ATEXPOSURE          : [Auto Exposure]              ",
                                                "TUIDC_HORIZONTAL          : [Horizontal]                 ",
                                                "TUIDC_VERTICAL            : [Vertical]                   ",
                                                "TUIDC_ATWBALANCE          : [Auto White Balance]         ",
                                                "TUIDC_FAN_GEAR            : [Fan Gear]                   ",
                                                "TUIDC_ATLEVELS            : [Auto Levels]                ",
                                                "TUIDC_SHIFT               : [Shift]                      ",
                                                "TUIDC_HISTC               : [Histogram Statistic]        ",
                                                "TUIDC_CHANNELS            : [Current Channel]            ",
                                                "TUIDC_ENHANCE             : [Enhance]                    ",
                                                "TUIDC_DFTCORRECTION       : [Defect Correction]          ",
                                                "TUIDC_ENABLEDENOISE       : [Enable Denoise]             ",
                                                "TUIDC_FLTCORRECTION       : [Flat Field Correction]      ",
                                                "TUIDC_RESTARTLONGTM       : [Restart Long Time]          ",
                                                "TUIDC_DATAFORMAT          : [Data format]                ",
                                                "TUIDC_DRCORRECTION        : [Dynamic Range Of Correction]",
                                                "TUIDC_VERCORRECTION       : [Vertical Correction]        ",
                                                "TUIDC_MONOCHROME          : [Monochromatic]              ",
                                                "TUIDC_BLACKBALANCE        : [Black Balance]              ",
                                                "TUIDC_IMGMODESELECT       : [Image mode(CMS)]            ",
                                                "TUIDC_CAM_MULTIPLE        : [Multiple Cameras]           ",
                                                "TUIDC_ENABLEPOWEEFREQUENCY: [Enable Power frequency]     ",
                                                "TUIDC_ROTATE_R90          : [Rotate Right 90°]          ",
                                                "TUIDC_ROTATE_L90          : [Rotate Left 90°]           ",
                                                "TUIDC_NEGATIVE            : [Enable Negative Film]       ",
                                                "TUIDC_HDR                 : [Enable HDR]                 ",
                                                "TUIDC_ENABLEIMGPRO        : [Enable Image Process]       ",
                                                "TUIDC_ENABLELED           : [Enable USB Led]             ",
                                                "TUIDC_ENABLETIMESTAMP     ; [Enable Time Stamp]          ",
                                                "TUIDC_ENABLEBLACKLEVEL    : [Enable Black Level]         ",

                                                "TUIDC_ATFOCUS             : [Auto Focus Enable]          ",
                                                "TUIDC_ATFOCUS_STATUS      : [Auto Focus Status]          ",
                                                "TUIDC_PGAGAIN             : [Sensor PGA Gain]            ",
                                                "TUIDC_ATEXPOSURE_MODE     : [Automatic Exposure Time Mode]",
                                                "TUIDC_BINNING_SUM         : [Summation Binning]          ",
                                                "TUIDC_BINNING_AVG         : [Average Binning]            ",
                                                "TUIDC_FOCUS_C_MOUNT       : [Focus c-mount Mode]         ",
                                                "TUIDC_ENABLEPI            : [PI Enable]                  ",
                                                "TUIDC_ATEXPOSURE_STATUS   : [Auto Exposure Status]       ",
                                                "TUIDC_ATWBALANCE_STATUS   : [Auto WhiteBalance Status]   ",
                                                "TUIDC_TESTIMGMODE         : [Test Image Mode Select]     ",
                                                "TUIDC_SENSORRESET         : [Sensor Reset]               ",
                                                "TUIDC_PGAHIGH             : [PGA High Gain]              ",
                                                "TUIDC_PGALOW              : [PGA Low Gain]               ",
                                                "TUIDC_PIXCLK1_EN          : [Pix2 Clock Enable]          ",
                                                "TUIDC_PIXCLK2_EN          : [Pix1 Clock Enable]          ",
                                                "TUIDC_ATLEVELGEAR         : [Auto Level Gear]            ",
                                                "TUIDC_ENABLEDSNU          : [Enable DSNU]                ",
                                                "TUIDC_ENABLEOVERLAP       : [Enable Exposure Time Overlap Mode]       ",
                                                "TUIDC_CAMSTATE            : [Camera State]               ",
                                                "TUIDC_ENABLETRIOUT        ; [Enable Trigger Out]         ",
                                                "TUIDC_ROLLINGSCANMODE     : [Rolling Scan Mode]          ",
                                                "TUIDC_ROLLINGSCANLTD      : [Rolling Scan Line Time Delay] ",
                                                "TUIDC_ROLLINGSCANSLIT     : [Rolling Scan Slit Height]     ",
                                                "TUIDC_ROLLINGSCANDIR      : [Rolling Scan Direction]       ",
                                                "TUIDC_ROLLINGSCANRESET    : [Rolling Scan Direction Reset] ",
                                                "TUIDC_ENABLETEC           : [Tec Enable]                   ",
                                                "TUIDC_ENABLEBLC           : [Backlight Compensation Enable]",
                                                "TUIDC_ENABLETHROUGHFOG    : [Electronic Through Fog Enable]",
                                                "TUIDC_ENABLEGAMMA         : [Gamma Enable]                 ",
                                                "TUIDC_ENABLEFILTER        ; [Filter Enable]                ",
                                                "TUIDC_ENABLEHLC           : [Strong Light Inhibition Enable]         ",
                                                "TUIDC_CAMPARASAVE         : [Camera Pamameter Save]       ",
                                                "TUIDC_CAMPARALOAD         : [Camera Parameter Load]       ",
                                                "TUIDC_ENABLEISP           : [Isp Enable]                  ",
                                                "TUIDC_BUFFERHEIGHT        : [Buffer Height]               ",
                                                "TUIDC_VISIBILITY          ; [Visibility]                  ",
                                                "TUIDC_SHUTTER             : [Shutter Mode]                ",
                                                "TUIDC_SIGNALFILTER        ; [Signal Filter]               "

                                              };

        /* Init the TUCAM API */
        static TUCAMRET InitApi() {
            /* Get the current directory */
            IntPtr strPath = Marshal.StringToHGlobalAnsi(System.Environment.CurrentDirectory);

            m_itApi.uiCamCount = 0;
            m_itApi.pstrConfigPath = strPath;

            TUCamAPI.TUCAM_Api_Init(ref m_itApi);

            Console.WriteLine("Connect {0} camera", m_itApi.uiCamCount);

            if (0 == m_itApi.uiCamCount) {
                return TUCAMRET.TUCAMRET_NO_CAMERA;
            }

            return TUCAMRET.TUCAMRET_SUCCESS;
        }

        /* UnInit the TUCAM API */
        static TUCAMRET UnInitApi() {
            return TUCamAPI.TUCAM_Api_Uninit();
        }

        /* Open the camera by index number */
        static TUCAMRET OpenCamera(uint uiIdx) {
            if (uiIdx >= m_itApi.uiCamCount) {
                return TUCAMRET.TUCAMRET_OUT_OF_RANGE;
            }

            m_opCam.uiIdxOpen = uiIdx;

            return TUCamAPI.TUCAM_Dev_Open(ref m_opCam);
        }

        /* Close the current camera */
        static TUCAMRET CloseCamera() {
            if (null != m_opCam.hIdxTUCam) {
                TUCamAPI.TUCAM_Dev_Close(m_opCam.hIdxTUCam);
            }

            Console.WriteLine("Close the camera success");

            return TUCAMRET.TUCAMRET_SUCCESS;
        }

        /* Print the camera capability list */
        static void PrintCameraCapabilityList() {
            int nVal = 0;

            List<string> lstCapa = new List<string>(m_strArrCapa);

            TUCAM_CAPA_ATTR capa;
            capa.idCapa = 0;
            capa.nValDft = 0;
            capa.nValMin = 0;
            capa.nValMax = 0;
            capa.nValStep = 0;

            TUCAM_VALUE_TEXT valText;
            valText.nID = 0;
            valText.dbValue = 0;
            valText.nTextSize = 64;
            valText.pText = Marshal.AllocHGlobal(64);

            /* Get capability list information */
            Console.WriteLine("Get capability information list");

            for (int i = (int)TUCAM_IDCAPA.TUIDC_RESOLUTION; i < (int)TUCAM_IDCAPA.TUIDC_ENDCAPABILITY; ++i) {
                capa.idCapa = i;

                if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Capa_GetAttr(m_opCam.hIdxTUCam, ref capa)) {
                    Console.WriteLine("0x{0:X2}.{1} Range[{2, 2}, {3, 2}] Default:{4, 2} Step:{5}", i, lstCapa[i].ToString(), capa.nValMin, capa.nValMax, capa.nValDft, capa.nValStep);

                    if ((int)TUCAM_IDCAPA.TUIDC_RESOLUTION == i)        /* Resolution */
                    {
                        int nCnt = capa.nValMax - capa.nValMin + 1;

                        valText.nID = i;

                        for (int j = 0; j < nCnt; ++j) {
                            valText.dbValue = j;
                            TUCAMRET n = TUCamAPI.TUCAM_Capa_GetValueText(m_opCam.hIdxTUCam, ref valText);
                            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Capa_GetValueText(m_opCam.hIdxTUCam, ref valText)) {
                                Console.WriteLine("{0}: {1}", j, Marshal.PtrToStringAnsi(valText.pText));
                            }
                        }
                    }
                    else if ((int)TUCAM_IDCAPA.TUIDC_PIXELCLOCK == i)    /* Pixel Clock */
                    {
                        int nCnt = capa.nValMax - capa.nValMin + 1;

                        valText.nID = i;

                        for (int j = 0; j < nCnt; ++j) {
                            valText.dbValue = j;
                            TUCAMRET n = TUCamAPI.TUCAM_Capa_GetValueText(m_opCam.hIdxTUCam, ref valText);
                            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Capa_GetValueText(m_opCam.hIdxTUCam, ref valText)) {
                                Console.WriteLine("{0}: {1}", j, Marshal.PtrToStringAnsi(valText.pText));
                            }
                        }
                    }
                }
                else {
                    Console.WriteLine("0x{0:X2}.{1} Not support", i, lstCapa[i].ToString());
                }

                Marshal.Release(valText.pText);
            }

            Console.WriteLine();

            /* Set capability default value */
            Console.WriteLine("Set capability default value");
            for (int i = (int)TUCAM_IDCAPA.TUIDC_RESOLUTION; i < (int)TUCAM_IDCAPA.TUIDC_ENDCAPABILITY; ++i) {
                capa.idCapa = i;

                if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Capa_GetAttr(m_opCam.hIdxTUCam, ref capa)) {
                    TUCamAPI.TUCAM_Capa_SetValue(m_opCam.hIdxTUCam, i, capa.nValDft);
                    Console.WriteLine("0x{0:X2}.{1} Set default value {2} success", i, lstCapa[i].ToString(), capa.nValDft);
                }
            }

            Console.WriteLine();

            /* Get capability current value */
            Console.WriteLine("Get capability current value");
            for (int i = (int)TUCAM_IDCAPA.TUIDC_RESOLUTION; i < (int)TUCAM_IDCAPA.TUIDC_ENDCAPABILITY; ++i) {
                if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Capa_GetValue(m_opCam.hIdxTUCam, i, ref nVal)) {
                    Console.WriteLine("0x{0:X2}.{1} The current value is {2}", i, lstCapa[i].ToString(), nVal);
                }
            }
        }

        static void Main(string[] args) {
            if (TUCAMRET.TUCAMRET_SUCCESS == InitApi()) {
                if (TUCAMRET.TUCAMRET_SUCCESS == OpenCamera(0)) {
                    Console.WriteLine("Open the camera success");

                    Console.WriteLine();
                    PrintCameraCapabilityList();
                    Console.WriteLine();

                    CloseCamera();
                }
                else {
                    Console.WriteLine("Open the camera failure");
                }

                UnInitApi();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
