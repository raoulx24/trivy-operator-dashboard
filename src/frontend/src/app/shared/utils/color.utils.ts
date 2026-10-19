export class ColorHelper {
  public static rainbow(numOfSteps: number, step: number) {
    // This function generates vibrant, "evenly spaced" colours (i.e. no clustering). This is ideal for creating easily distinguishable vibrant markers in Google Maps and other apps.
    // Adam Cole, 2011-Sept-14
    // HSV to RBG adapted from: http://mjijackson.com/2008/02/rgb-to-hsl-and-rgb-to-hsv-color-model-conversion-algorithms-in-javascript
    // source: https://stackoverflow.com/questions/1484506/random-color-generator
    let r = 0,
      g = 0,
      b = 0;
    const h = step / numOfSteps;
    const i = ~~(h * 6);
    const f = h * 6 - i;
    const q = 1 - f;
    switch (i % 6) {
      case 0:
        r = 1;
        g = f;
        b = 0;
        break;
      case 1:
        r = q;
        g = 1;
        b = 0;
        break;
      case 2:
        r = 0;
        g = 1;
        b = f;
        break;
      case 3:
        r = 0;
        g = q;
        b = 1;
        break;
      case 4:
        r = f;
        g = 0;
        b = 1;
        break;
      case 5:
        r = 1;
        g = 0;
        b = q;
        break;
    }
    const c =
      '#' +
      ('00' + (~~(r * 255)).toString(16)).slice(-2) +
      ('00' + (~~(g * 255)).toString(16)).slice(-2) +
      ('00' + (~~(b * 255)).toString(16)).slice(-2);
    return c;
  }
}
