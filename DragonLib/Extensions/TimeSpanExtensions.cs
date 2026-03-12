// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.Extensions;

public static class TimeSpanExtensions {
	extension(TimeSpan time) {
		public static TimeSpan OneNanosecond => TimeSpan.FromMicroseconds(0.001);
		public static TimeSpan OneMicrosecond => TimeSpan.FromMicroseconds(1);
		public static TimeSpan OneMillisecond => TimeSpan.FromMilliseconds(1);
		public static TimeSpan OneSecond => TimeSpan.FromSeconds(1);
		public static TimeSpan OneMinute => TimeSpan.FromMinutes(1);
		public static TimeSpan OneHour => TimeSpan.FromHours(1);
		public static TimeSpan OneDay => TimeSpan.FromDays(1);
		public static TimeSpan OneMonth => TimeSpan.FromDays(30);
		public static TimeSpan OneYear => TimeSpan.FromDays(356);
		public static TimeSpan OneDecade => TimeSpan.OneYear * 10;
		public static TimeSpan OneCentury => TimeSpan.OneDecade * 10;
		public static TimeSpan OneKiloyear => TimeSpan.OneCentury * 10;

		public string RelativeTime => time.GetHumanReadableTime();
		public string ShortRelativeTime => time.GetHumanReadableTime(true);

		public string GetHumanReadableTime(bool shortForm = false) {
			long amount;
			string metric;
			if (time >= TimeSpan.OneKiloyear) {
				amount = (long) Math.Floor(time / TimeSpan.OneKiloyear);
				metric = shortForm ? "kyr" : "kiloyear";
			} else if (time >= TimeSpan.OneCentury) {
				amount = (long) Math.Floor(time / TimeSpan.OneCentury);
				metric = shortForm ? "c" : "century";
			} else if (time >= TimeSpan.OneDecade) {
				amount = (long) Math.Floor(time / TimeSpan.OneDecade);
				metric = shortForm ? "s" : "decade";
			} else if (time >= TimeSpan.OneYear) {
				amount = (long) Math.Floor(time / TimeSpan.OneYear);
				metric = shortForm ? "y" : "year";
			} else if (time >= TimeSpan.OneMonth) {
				amount = (long) Math.Floor(time / TimeSpan.OneMonth);
				metric = shortForm ? "mo" : "month";
			} else if (time >= TimeSpan.OneDay) {
				amount = (long) Math.Floor(time / TimeSpan.OneDay);
				metric = shortForm ? "d" : "day";
			} else if (time >= TimeSpan.OneHour) {
				amount = (long) Math.Floor(time / TimeSpan.OneHour);
				metric = shortForm ? "h" : "hour";
			} else if (time >= TimeSpan.OneMinute) {
				amount = (long) Math.Floor(time / TimeSpan.OneMinute);
				metric = shortForm ? "m" : "minute";
			} else if (time >= TimeSpan.OneSecond) {
				amount = (long) Math.Floor(time / TimeSpan.OneSecond);
				metric = shortForm ? "s" : "second";
			} else if (time >= TimeSpan.OneMillisecond) {
				amount = (long) Math.Floor(time / TimeSpan.OneMillisecond);
				metric = shortForm ? "ms" : "millisecond";
			} else if (time >= TimeSpan.OneMicrosecond) {
				amount = (long) Math.Floor(time / TimeSpan.OneMicrosecond);
				metric = shortForm ? "us" : "microsecond";
			} else if (time >= TimeSpan.OneNanosecond) {
				amount = (long) Math.Floor(time / TimeSpan.OneNanosecond);
				metric = shortForm ? "ns" : "nanosecond";
			} else {
				amount = time.Ticks;
				metric = shortForm ? "t" : "tick";
			}

			if (shortForm) {
				return $"{amount}{metric}";
			}

			if (amount != 1) {
				metric += "s";
			}

			return $"{amount} {metric}";
		}
	}
}
