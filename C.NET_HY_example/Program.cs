using System.Runtime.InteropServices;
using TUCAMAPI;

namespace C.NET_HY_example {
    class Program {
        public static TUCAM_INIT m_itApi;       // SDK API initialized object
        public static TUCAM_OPEN m_opCam;       // Open camera object
        public static TUCAM_VALUE_INFO m_viCam; // Value info object
        public static TUCAM_REG_RW m_regRW;     // Register read/write
        public static TUCAM_FRAME m_frame;       // The frame object
        public static TUCAM_TRIGGER_ATTR m_tgr; // The trigger object
        public static TUCAM_FILE_SAVE m_fs;     // The file save object

        enum Cf {
            Temperature, Gain, ExposureTime, CMS, ROI,//Properties
            FanGear, //capability
        }
        enum RoiMode {
            P3000,
            P2000,
            P1000,
            P600,
        }
        enum FanGear {
            Fast = 0,
            Medium = 1,
            Slow = 2
        }
        enum CMS { //CMS, Correlated Multi-Sampling mode 
            Standard = 0,
            LowNoise = 1,
        }

        /* Wait for image data */
        static void AverageValue() {
            m_frame.pBuffer = IntPtr.Zero;
            m_frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl;
            m_frame.uiRsdSize = 1;

            TUCamAPI.TUCAM_Buf_Alloc(m_opCam.hIdxTUCam, ref m_frame);
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_Start(m_opCam.hIdxTUCam, (uint)TUCAM_CAPTURE_MODES.TUCCM_SEQUENCE)) {
                if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Buf_WaitForFrame(m_opCam.hIdxTUCam, ref m_frame)) {

                    // Calculate the average gray value of the full-frame area or ROI
                    bool isRoi = false;

                    //ROI parameters
                    int roiX = 200;
                    int roiY = 400;
                    int roiW = 800;
                    int roiH = 600;

                    int startX = isRoi ? roiX : 0;
                    int startY = isRoi ? roiY : 0;
                    int width = isRoi ? roiW : m_frame.usWidth;
                    int height = isRoi ? roiH : m_frame.usHeight;
                    int finishX = startX + width;
                    int finishY = startY + height;
                    int pixels = width * height;
                    double avg = 0;
                    long sum = 0;

                    unsafe {
                        if (2 == m_frame.ucElemBytes) {
                            ushort* data = (ushort*)(m_frame.pBuffer + m_frame.usHeader);
                            for (int k = startY; k < finishY; ++k) {
                                for (int j = startX; j < finishX; ++j) {
                                    sum += data[k * width + j];
                                }
                            }
                        }
                    }
                    avg = sum / pixels;
                    Console.WriteLine("Average value is {0}", avg);
                }
                else {
                    Console.WriteLine("Grab the frame failure");
                }


                TUCamAPI.TUCAM_Buf_AbortWait(m_opCam.hIdxTUCam);
                TUCamAPI.TUCAM_Cap_Stop(m_opCam.hIdxTUCam);
            }

            TUCamAPI.TUCAM_Buf_Release(m_opCam.hIdxTUCam);
        }


        /// <summary>
        /// Get the information of CMOS camera
        /// </summary>
        /// <param name="property">"Temperature","Gain", "Exposure time", "Fan Gear"</param>
        static double GetConfig(Enum property) {

            double ret = 0f;
            int propertyID;

            TUCAM_CAPA_ATTR capa;
            capa.idCapa = 0;
            capa.nValDft = 0;
            capa.nValMin = 0;
            capa.nValMax = 0;
            capa.nValStep = 0;
            int nVal = 0;

            List<string> lstCapa = new List<string>(m_strArrCapa);

            switch (property) {
                case Cf.ROI:

                    // mulitples of 4, 3000x3000
                    TUCAM_ROI_ATTR roi;
                    roi.bEnable = true;
                    roi.nHOffset = 0;
                    roi.nVOffset = 0;
                    roi.nHeight = 0;
                    roi.nWidth = 0;

                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_GetROI(m_opCam.hIdxTUCam, ref roi)) {
                        Console.WriteLine("Get ROI state success, HOffset:{0}, VOffset:{1}, Width:{2}, Height:{3}", roi.nHOffset, roi.nVOffset, roi.nWidth, roi.nHeight);
                    }
                    else {
                        Console.WriteLine("Get ROI state failure, HOffset:{0}, VOffset:{1}, Width:{2}, Height:{3}", roi.nHOffset, roi.nVOffset, roi.nWidth, roi.nHeight);
                    }

                    break;
                case Cf.Temperature:
                    propertyID = (int)TUCAM_IDPROP.TUIDP_TEMPERATURE;
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Prop_GetValue(m_opCam.hIdxTUCam, propertyID, ref ret, 0)) {
                        Console.WriteLine("Temperature : {0, 6:f2} degree", ret);
                    }
                    break;
                case Cf.Gain:
                    propertyID = (int)TUCAM_IDPROP.TUIDP_GLOBALGAIN;
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Prop_GetValue(m_opCam.hIdxTUCam, propertyID, ref ret, 0)) {
                        Console.WriteLine("Gain : {0}", ret);
                    }
                    break;
                case Cf.ExposureTime:
                    propertyID = (int)TUCAM_IDPROP.TUIDP_EXPOSURETM;
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Prop_GetValue(m_opCam.hIdxTUCam, propertyID, ref ret, 0)) {
                        Console.WriteLine("Exposure time : {0, 6:f2} ms", ret);
                    }
                    break;
                case Cf.CMS:
                    capa.idCapa = (int)TUCAM_IDCAPA.TUIDC_IMGMODESELECT;
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Capa_GetValue(m_opCam.hIdxTUCam, capa.idCapa, ref nVal)) {
                        Console.WriteLine("CMS value : {0}", nVal);
                    }
                    ret = nVal;
                    break;
                case Cf.FanGear:
                    capa.idCapa = (int)TUCAM_IDCAPA.TUIDC_FAN_GEAR;
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Capa_GetValue(m_opCam.hIdxTUCam, capa.idCapa, ref nVal)) {
                        string rep = (nVal == 0) ? "Fast" :
                                     (nVal == 1) ? "Medium" : "Low";
                        Console.WriteLine("FanGear : {0}", rep);
                    }
                    ret = nVal;
                    break;
                default:
                    Console.WriteLine("Please select the Cfuration item");
                    return 0;
            }
            return ret;
        }

        static void SetConfig(Enum property, Enum setValue) {

            TUCAM_CAPA_ATTR capa;
            capa.idCapa = 0; capa.nValDft = 0; capa.nValMin = 0;
            capa.nValMax = 0; capa.nValStep = 0;

            int value = Convert.ToInt32(setValue);
            switch (property) {
                case Cf.FanGear: //setValue : 0-Fast, 1-Medium, 2-Slow
                    capa.idCapa = (int)TUCAM_IDCAPA.TUIDC_FAN_GEAR;
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Capa_GetAttr(m_opCam.hIdxTUCam, ref capa)) {
                        TUCamAPI.TUCAM_Capa_SetValue(m_opCam.hIdxTUCam, capa.idCapa, value);
                        string rep = (value == 0) ? "Fast" :
                                     (value == 1) ? "Medium" : "Low";
                        Console.WriteLine("FanGear set to be {0}", rep);
                    }
                    break;
                case Cf.CMS: //setValue : 0- standard, 1- low noise
                    capa.idCapa = (int)TUCAM_IDCAPA.TUIDC_IMGMODESELECT;
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Capa_GetAttr(m_opCam.hIdxTUCam, ref capa)) {
                        TUCamAPI.TUCAM_Capa_SetValue(m_opCam.hIdxTUCam, capa.idCapa, value);
                        string rep = (value == 0) ? "Standard" : "Low noise";
                        Console.WriteLine("CMS mode set to be {0}", rep);
                    }
                    break;
                case Cf.ROI: // Pixels * Pixels : "P3000", "P2000", "P1000", "P600"
                    // mulitples of 4, 3000x3000
                    TUCAM_ROI_ATTR roi;
                    roi.bEnable = true;
                    switch (setValue) {
                        case RoiMode.P3000:
                            roi.nHOffset = 0; roi.nVOffset = 0;
                            roi.nWidth = 3000; roi.nHeight = 3000;
                            break;
                        case RoiMode.P2000:
                            roi.nHOffset = 500; roi.nVOffset = 500;
                            roi.nWidth = 2000; roi.nHeight = 2000;
                            break;
                        case RoiMode.P1000:
                            roi.nHOffset = 1000; roi.nVOffset = 1000;
                            roi.nWidth = 1000; roi.nHeight = 1000;
                            break;
                        case RoiMode.P600:
                            roi.nHOffset = 1200; roi.nVOffset = 1200;
                            roi.nWidth = 600; roi.nHeight = 600;
                            break;
                        default:
                            roi.nHOffset = 0; roi.nVOffset = 0;
                            roi.nWidth = 3000; roi.nHeight = 3000;
                            break;
                    }
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_SetROI(m_opCam.hIdxTUCam, roi)) {
                        Console.WriteLine("Set ROI state success, HOffset:{0}, VOffset:{1}, Width:{2}, Height:{3}", roi.nHOffset, roi.nVOffset, roi.nWidth, roi.nHeight);
                    }
                    else {
                        Console.WriteLine("Set ROI state failure, HOffset:{0}, VOffset:{1}, Width:{2}, Height:{3}", roi.nHOffset, roi.nVOffset, roi.nWidth, roi.nHeight);
                    }
                    break;
                default:
                    return;
            }


        }
        static void SetConfig(Enum property, double ms) {
            int propertyID = (int)TUCAM_IDPROP.TUIDP_EXPOSURETM;

            switch (property) {
                case Cf.ExposureTime:
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Prop_SetValue(m_opCam.hIdxTUCam, propertyID, ms)) {
                        Console.WriteLine("Exposure time set to be {0} ms", ms);
                    }
                    break;
                default:
                    return;
            }
        }

        static void DoSoftwareTrigger() {
            int nTimes = 10;

            m_frame.pBuffer = IntPtr.Zero;
            m_frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl;
            m_frame.uiRsdSize = 1;

            TUCamAPI.TUCAM_Cap_GetTrigger(m_opCam.hIdxTUCam, ref m_tgr);
            m_tgr.nTgrMode = (int)TUCAM_CAPTURE_MODES.TUCCM_TRIGGER_SOFTWARE;
            m_tgr.nFrames = 1;
            m_tgr.nBufFrames = 1;
            TUCamAPI.TUCAM_Cap_SetTrigger(m_opCam.hIdxTUCam, m_tgr);

            TUCamAPI.TUCAM_Buf_Alloc(m_opCam.hIdxTUCam, ref m_frame);

            long lRet = (long)TUCAMRET.TUCAMRET_NOT_SUPPORT;

            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_Start(m_opCam.hIdxTUCam, (uint)TUCAM_CAPTURE_MODES.TUCCM_TRIGGER_SOFTWARE)) {
                for (int i = 0; i < nTimes; ++i) {
                    Thread.Sleep(100);
                    /* Send software trigger signal */
                    lRet = (long)TUCamAPI.TUCAM_Cap_DoSoftwareTrigger(m_opCam.hIdxTUCam);

                    if ((int)TUCAMRET.TUCAMRET_SUCCESS == lRet) {
                        if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Buf_WaitForFrame(m_opCam.hIdxTUCam, ref m_frame)) {
                            Console.WriteLine("Grab the software trigger frame success, index number is {0}, width:{1}, height:{2}, channel:{3}, depth:{4}, image size:{5}", i, m_frame.usWidth, m_frame.usHeight, m_frame.ucChannels, (2 == m_frame.ucElemBytes) ? 16 : 8, m_frame.uiImgSize);
                        }
                        else {
                            Console.WriteLine("Grab the software trigger frame failure, index number is {0}", i);
                        }
                    }
                    else if ((long)TUCAMRET.TUCAMRET_NOT_SUPPORT == lRet) {
                        Console.WriteLine("This camera cannot support software trigger");
                        break;
                    }
                    else {
                        Console.WriteLine("Send the software trigger signal failure");
                    }
                }

                TUCamAPI.TUCAM_Buf_AbortWait(m_opCam.hIdxTUCam);
                TUCamAPI.TUCAM_Cap_Stop(m_opCam.hIdxTUCam);
            }

            TUCamAPI.TUCAM_Buf_Release(m_opCam.hIdxTUCam);
        }

        static void DoHardwareTrigger() {
            int nTimes = 10;

            m_frame.pBuffer = IntPtr.Zero;
            m_frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl;
            m_frame.uiRsdSize = 1;

            TUCamAPI.TUCAM_Cap_GetTrigger(m_opCam.hIdxTUCam, ref m_tgr);
            m_tgr.nTgrMode = (int)TUCAM_CAPTURE_MODES.TUCCM_TRIGGER_STANDARD;
            /* how many frames do you want to capture to RAM(the frames less than 0, use maximum frames ) */
            m_tgr.nFrames = 1; // The number of output frames triggered at one time
            m_tgr.nBufFrames = 1; //The number of frames in the buffer
            m_tgr.nDelayTm = 0; // trigger delay time in milliseconds
            m_tgr.nExpMode = (int)TUCAM_TRIGGER_EXP.TUCTE_EXPTM; //exposure time
            m_tgr.nEdgeMode = (int)TUCAM_TRIGGER_EDGE.TUCTD_RISING; // rising edge
            TUCamAPI.TUCAM_Cap_SetTrigger(m_opCam.hIdxTUCam, m_tgr);

            TUCamAPI.TUCAM_Buf_Alloc(m_opCam.hIdxTUCam, ref m_frame);

            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_Start(m_opCam.hIdxTUCam, (uint)TUCAM_CAPTURE_MODES.TUCCM_TRIGGER_STANDARD)) {
                for (int i = 0; i < nTimes; ++i) {
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Buf_WaitForFrame(m_opCam.hIdxTUCam, ref m_frame)) {
                        /* After call TUCAM_Buf_WaitForFrame your hardware can send the trigger signal */
                        /* if m_tgr.nFrames == 1, the SDK create 1 frame memory to receive one trigger signal and the TUCAM_Buf_WaitForFrame interface return once */
                        /* if m_tgr.nFrames == N, the SDK create N frames memory to receive N times trigger signals, the TUCAM_Buf_WaitForFrame interface return, and you call TUCAM_Buf_WaitForFrame N-1 times to grab the frame */
                        /* if m_tgr.nFrames < 0, the SDK create Max frames memory to receive trigger signals */

                        Console.WriteLine("Grab the hardware trigger frame success, index number is {0}, width:{1}, height:{2}, channel:{3}, depth:{4}, image size:{5}", i, m_frame.usWidth, m_frame.usHeight, m_frame.ucChannels, (2 == m_frame.ucElemBytes) ? 16 : 8, m_frame.uiImgSize);
                    }
                    else {
                        Console.WriteLine("Grab the hardware trigger frame failure, index number is {0}", i);
                    }
                }

                TUCamAPI.TUCAM_Buf_AbortWait(m_opCam.hIdxTUCam);
                TUCamAPI.TUCAM_Cap_Stop(m_opCam.hIdxTUCam);
            }

            TUCamAPI.TUCAM_Buf_Release(m_opCam.hIdxTUCam);
        }

        /* Save image data */
        static void SaveImageData() {
            string strPath;
            int nTimes = 10;

            m_frame.pBuffer = IntPtr.Zero;
            m_frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl;
            m_frame.uiRsdSize = 1;

            m_fs.nSaveFmt = (int)TUIMG_FORMATS.TUFMT_TIF;

            TUCamAPI.TUCAM_Buf_Alloc(m_opCam.hIdxTUCam, ref m_frame);

            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_Start(m_opCam.hIdxTUCam, (uint)TUCAM_CAPTURE_MODES.TUCCM_SEQUENCE)) {
                for (int i = 0; i < nTimes; ++i) {
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Buf_WaitForFrame(m_opCam.hIdxTUCam, ref m_frame)) {
                        strPath = string.Format(".\\{0}", i);

                        m_fs.pstrSavePath = Marshal.StringToHGlobalAnsi(strPath);       /* path */
                        m_fs.pFrame = Marshal.AllocHGlobal(Marshal.SizeOf(m_frame));
                        Marshal.StructureToPtr(m_frame, m_fs.pFrame, true);             /* struct to IntPtr */

                        if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_File_SaveImage(m_opCam.hIdxTUCam, m_fs)) {
                            Console.WriteLine("Save the image data success, the path is {0}.tiff", strPath);
                        }
                        else {
                            Console.WriteLine("Save the image data failure, the path is {0}.tiff", strPath);
                        }
                    }
                    else {
                        Console.WriteLine("Grab the frame failure, index number is {0}", i);
                    }
                }

                TUCamAPI.TUCAM_Buf_AbortWait(m_opCam.hIdxTUCam);
                TUCamAPI.TUCAM_Cap_Stop(m_opCam.hIdxTUCam);
            }

            TUCamAPI.TUCAM_Buf_Release(m_opCam.hIdxTUCam);
        }


        static void Main(string[] args) {
            if (TUCAMRET.TUCAMRET_SUCCESS == InitApi()) {
                if (TUCAMRET.TUCAMRET_SUCCESS == OpenCamera(0)) {
                    Console.WriteLine("Open the camera success");

                    SetConfig(Cf.FanGear, FanGear.Slow);
                    SetConfig(Cf.CMS, CMS.LowNoise);
                    SetConfig(Cf.ExposureTime, 100);
                    SetConfig(Cf.ROI, RoiMode.P600);

                    Console.WriteLine(); Console.WriteLine();
                    GetConfig(Cf.ROI);
                    GetConfig(Cf.Temperature);
                    GetConfig(Cf.Gain);
                    GetConfig(Cf.FanGear);
                    GetConfig(Cf.ExposureTime);

                    Console.WriteLine();
                    AverageValue();
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


        #region parameter
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
        #endregion

        #region Init&Open


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

        #endregion

        #region Print Camera Info
        /* Print the camera information */
        static void PrintCameraInfo() {
            string strVal;
            string strText;
            IntPtr pText = Marshal.AllocHGlobal(64);

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_CAMERA_MODEL;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam)) {
                strText = Marshal.PtrToStringAnsi(m_viCam.pText);

                Console.WriteLine("Camera  Name    : {0}", strText);
            }

            m_regRW.nRegType = (int)TUREG_TYPE.TUREG_SN;
            m_regRW.nBufSize = 64;
            m_regRW.pBuf = pText;

            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Reg_Read(m_opCam.hIdxTUCam, m_regRW)) {
                strText = Marshal.PtrToStringAnsi(m_regRW.pBuf);
                Console.WriteLine("Camera  SN      : {0}", strText);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_VENDOR;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam)) {
                strVal = String.Format("{0:X000}", m_viCam.nValue);
                Console.WriteLine("Camera  VID     : 0x{0}", strVal);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_PRODUCT;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam)) {
                strVal = String.Format("{0:X000}", m_viCam.nValue);
                Console.WriteLine("Camera  PID     : 0x{0}", strVal);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_CAMERA_CHANNELS;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam)) {
                Console.WriteLine("Camera  Channels: {0}", m_viCam.nValue);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_BUS;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam)) {
                if (0x200 == m_viCam.nValue || 0x210 == m_viCam.nValue) {
                    Console.WriteLine("USB     Type    : {0}", "2.0");
                }
                else {
                    Console.WriteLine("USB     Type    : {0}", "3.0");
                }
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_VERSION_FRMW;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam)) {
                strVal = String.Format("{0:X000}", m_viCam.nValue);
                Console.WriteLine("Version Firmware: 0x{0}", strVal);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_VERSION_API;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam)) {
                strText = Marshal.PtrToStringAnsi(m_viCam.pText);
                Console.WriteLine("Version API     : {0}", strText);
            }

            Marshal.Release(pText);
        }
        /* Print the camera information by index number */
        static void PrintCameraInfoEx(uint uiIdx) {
            if (uiIdx >= m_itApi.uiCamCount) {
                Console.WriteLine("PrintCameraInfoEx: The index number of camera is out of range.");
                return;
            }

            string strVal;
            string strText;
            IntPtr pText = Marshal.AllocHGlobal(64);

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_CAMERA_MODEL;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfoEx(uiIdx, ref m_viCam)) {
                strText = Marshal.PtrToStringAnsi(m_viCam.pText);

                Console.WriteLine("Camera  Name    : {0}", strText);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_VENDOR;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfoEx(uiIdx, ref m_viCam)) {
                strVal = String.Format("{0:X000}", m_viCam.nValue);
                Console.WriteLine("Camera  VID     : 0x{0}", strVal);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_PRODUCT;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfoEx(uiIdx, ref m_viCam)) {
                strVal = String.Format("{0:X000}", m_viCam.nValue);
                Console.WriteLine("Camera  PID     : 0x{0}", strVal);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_BUS;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfoEx(uiIdx, ref m_viCam)) {
                if (0x200 == m_viCam.nValue || 0x210 == m_viCam.nValue) {
                    Console.WriteLine("USB     Type    : {0}", "2.0");
                }
                else {
                    Console.WriteLine("USB     Type    : {0}", "3.0");
                }
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_VERSION_API;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfoEx(uiIdx, ref m_viCam)) {
                strText = Marshal.PtrToStringAnsi(m_viCam.pText);
                Console.WriteLine("Version API     : {0}", strText);
            }
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
        #endregion

    }
}