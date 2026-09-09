using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Codecs
{

    class H264
    {
        const byte ProfileConstrainedBaseline = 1;
        const byte ProfileBaseline = 2;
        const byte ProfileMain = 3;
        const byte ProfileConstrainedHigh = 4;
        const byte ProfileHigh = 5;

        const byte Level1_b = 0;
        const byte Level1 = 10;
        const byte Level1_1 = 11;
        const byte Level1_2 = 12;
        const byte Level1_3 = 13;
        const byte Level2 = 20;
        const byte Level2_1 = 21;
        const byte Level2_2 = 22;
        const byte Level3 = 30;
        const byte Level3_1 = 31;
        const byte Level3_2 = 32;
        const byte Level4 = 40;
        const byte Level4_1 = 41;
        const byte Level4_2 = 42;
        const byte Level5 = 50;
        const byte Level5_1 = 51;
        const byte Level5_2 = 52;

        const byte ConstraintSet3Flag = 0x10;


        public class ProfileLevelId
        {
            public byte Profile { get; set; }
            public byte Level { get; set; }
        }

        static ProfileLevelId _defaultProfileLevelId = new()
        {
            Profile = ProfileConstrainedBaseline,
            Level = Level3_1
        };

        public class BitPattern
        {
            public BitPattern(string str)
            {
                Mask = (byte)~ByteMaskString('x', str);
                MaskedValue = ByteMaskString('1', str);
            }

            public byte Mask { get; set; }
            public byte MaskedValue { get; set; }

            public bool IsMatch(byte value)
            {
                return MaskedValue == (value & Mask);
            }
        }

        public class ProfilePattern
        {
            public ProfilePattern(byte profileIdc, BitPattern profileIop, byte profile)
            {
                ProfileIdc = profileIdc;
                ProfileIop = profileIop;
                Profile = profile;
            }

            public byte ProfileIdc { get; set; }
            public BitPattern ProfileIop { get; set; }
            public byte Profile { get; set; }
        }

        static public ProfilePattern[] ProfilePatterns = new ProfilePattern[]
        {
            new ProfilePattern(0x42, new BitPattern("x1xx0000"), ProfileConstrainedBaseline),
            new ProfilePattern(0x4D, new BitPattern("1xxx0000"), ProfileConstrainedBaseline),
            new ProfilePattern(0x58, new BitPattern("11xx0000"), ProfileConstrainedBaseline),
            new ProfilePattern(0x42, new BitPattern("x0xx0000"), ProfileBaseline),
            new ProfilePattern(0x58, new BitPattern("10xx0000"), ProfileBaseline),
            new ProfilePattern(0x4D, new BitPattern("0x0x0000"), ProfileMain),
            new ProfilePattern(0x64, new BitPattern("00000000"), ProfileHigh),
            new ProfilePattern(0x64, new BitPattern("00001100"), ProfileConstrainedHigh)
        };


        //public static bool IsSameProfile(H264Parameters params1, H264Parameters params2)
        //{
        //    return params1.ProfileLevelId is not null && params2.ProfileLevelId is not null &&
        //        params1.ProfileLevelId == params2.ProfileLevelId;
        //}

        public static bool IsSameProfile(Dictionary<string, object/*string*/> params1, Dictionary<string, object/*string*/> params2)
        {
            var profileLevelId1 = ParseSdpProfileLevelId(params1);
            var profileLevelId2 = ParseSdpProfileLevelId(params2);

            return profileLevelId1 is not null && profileLevelId2 is not null &&
                profileLevelId1.Profile == profileLevelId2.Profile;

        }

        //public static string GenerateProfileLevelIdForAnswer(H264Parameters localSupportedParams, 
        //    H264Parameters remoteOfferedParams)
        //{
        //    if (localSupportedParams.ProfileLevelId is null && remoteOfferedParams.ProfileLevelId is null)
        //        return null;


        //    var localProfileLevelId = ParseSdpProfileLevelId(localSupportedParams);
        //    var remoteProfileLevelId = ParseSdpProfileLevelId(remoteOfferedParams);

        //    if (localProfileLevelId is null || remoteProfileLevelId is null
        //        || localProfileLevelId != remoteProfileLevelId)
        //        return null;

        //    var levelAsymmetryAllowed = IsLevelAsymmetryAllowed(localSupportedParams)
        //        && IsLevelAsymmetryAllowed(remoteOfferedParams);

        //    byte localLevel = localProfileLevelId.Level;
        //    byte remoteLevel = remoteProfileLevelId.Level;
        //    byte minLevel = MinLevel(localLevel, remoteLevel);

        //    byte answerLevel = levelAsymmetryAllowed ? localLevel : minLevel;

        //    return ProfileLevelIdToString(new ProfileLevelId
        //    {
        //        Profile = localProfileLevelId.Profile,
        //        Level = answerLevel
        //    });
        //}

        public static string GenerateProfileLevelIdForAnswer(Dictionary<string, object/*string*/> localSupportedParams,
            Dictionary<string, object/*string*/> remoteOfferedParams)
        {
            if (!localSupportedParams.ContainsKey("profile-level-id") && 
                !remoteOfferedParams.ContainsKey("profile-level-id"))
            {
                Console.WriteLine("generateProfileLevelIdForAnswer() | no profile-level-id in local and remote params");
                return null;
            }

            var localProfileLevelId = ParseSdpProfileLevelId(localSupportedParams);
            var remoteProfileLevelId = ParseSdpProfileLevelId(remoteOfferedParams);

            if (localProfileLevelId is null || remoteProfileLevelId is null
                || localProfileLevelId.Profile != remoteProfileLevelId.Profile)
                return null;

            var levelAsymmetryAllowed = IsLevelAsymmetryAllowed(localSupportedParams)
                && IsLevelAsymmetryAllowed(remoteOfferedParams);

            byte localLevel = localProfileLevelId.Level;
            byte remoteLevel = remoteProfileLevelId.Level;
            byte minLevel = MinLevel(localLevel, remoteLevel);

            byte answerLevel = levelAsymmetryAllowed ? localLevel : minLevel;

            return ProfileLevelIdToString(new ProfileLevelId
            {
                Profile = localProfileLevelId.Profile,
                Level = answerLevel
            });
        }

        //public static ProfileLevelId ParseSdpProfileLevelId(H264Parameters params_)
        //{
        //    return params_.ProfileLevelId is null ? _defaultProfileLevelId : 
        //        ParseProfileLevelId(params_.ProfileLevelId);
        //}

        public static ProfileLevelId ParseSdpProfileLevelId(Dictionary<string, object/*string*/> params_)
        {
            return params_.ContainsKey("profile-level-id") ?
                ParseProfileLevelId(params_["profile-level-id"] as string) : _defaultProfileLevelId;
        }

        public static string ProfileLevelIdToString(ProfileLevelId profileLevelId)
        {

            // Handle special case level 1b.
            if (profileLevelId.Level == Level1_b)
            {
                return profileLevelId.Profile switch
                {
                    ProfileConstrainedBaseline => "42f00b",
                    ProfileBaseline => "42100b",
                    ProfileMain => "4d100b",
                    _ => null
                };
            }

            string profileIdcIopString = profileLevelId.Profile switch
            {
                ProfileConstrainedBaseline => "42e0",
                ProfileBaseline => "4200",
                ProfileMain => "4d00",
                ProfileConstrainedHigh => "640c",
                ProfileHigh => "6400",
                _ => null
            };
            if (profileIdcIopString is null)
                return null;

            string levelStr = profileLevelId.Level.ToString("x2");

            return profileIdcIopString + levelStr;
        }


        public static ProfileLevelId ParseProfileLevelId(string str)
        {
            // The string should consist of 3 bytes in hexadecimal format.
            if (str.Length != 6)
                return null;

            int profileLevelIdNumeric = Convert.ToInt32(str, 16);
            if (profileLevelIdNumeric == 0)
                return null;

            byte levelIdc = (byte)(profileLevelIdNumeric & 0xff);
            byte profileIop = (byte)((profileLevelIdNumeric >> 8) & 0xff);
            byte profileIdc = (byte)((profileLevelIdNumeric >> 16) & 0xff);

            byte level = levelIdc switch
            {
                Level1_1 => (profileIop & ConstraintSet3Flag) != 0 ? Level1_b : Level1_1,
                Level1 => Level1,
                Level1_2 => Level1_2,
                Level1_3 => Level1_3,
                Level2 => Level2,
		        Level2_1 => Level2_1,
		        Level2_2 => Level2_2,
		        Level3 => Level3,
		        Level3_1 => Level3_1,
		        Level3_2 => Level3_2,
		        Level4 => Level4,
		        Level4_1 => Level4_1,
		        Level4_2 => Level4_2,
		        Level5 => Level5,
		        Level5_1 => Level5_1,
		        Level5_2 => Level5_2,
                _ => 0xff
            };

            if (level == 0xff)
                return null;

            foreach (var pattern in ProfilePatterns)
            {
                if (profileIdc == pattern.ProfileIdc && pattern.ProfileIop.IsMatch(profileIop))
                    return new ProfileLevelId
                    {
                        Profile = pattern.Profile,
                        Level = level
                    };
            }

            return null;
        }

        // Convert a string of 8 characters into a byte where the positions containing
        // character c will have their bit set. For example, c = 'x', str = "x1xx0000"
        // will return 0b11110000.
        static byte ByteMaskString(char c, string str)
        {
            return (byte)(
                ((str[0] == c) ? 1<<7 : 0) | ((str[1] == c) ? 1<<6 : 0) | ((str[2] == c) ? 1<<5 : 0) |
                ((str[3] == c) ? 1<<4 : 0) | ((str[4] == c) ? 1<<3 : 0) | ((str[5] == c) ? 1<<2 : 0) |
                ((str[6] == c) ? 1<<1 : 0) | ((str[7] == c) ? 1<<0 : 0)
            );
        }

        //static bool IsLevelAsymmetryAllowed(H264Parameters params_)
        //{
        //    return params_.LevelAsymmetryAllowed is not null && params_.LevelAsymmetryAllowed == 1;
        //}

        static bool IsLevelAsymmetryAllowed(Dictionary<string, object/*string*/> params_)
        {
            return params_.ContainsKey("level-asymmetry-allowed") ? (int)params_["level-asymmetry-allowed"] == 1 : false;
        }

        static bool IsLessLevel(byte a, byte b)
        {
            if (a == Level1_b)
                return b != Level1 && b != Level1_b;

            if (b == Level1_b)
                return a != Level1;

            return a < b;
        }

        static byte MinLevel(byte a , byte b)
        {
            return IsLessLevel(a, b) ? a : b;
        }
    }
}
